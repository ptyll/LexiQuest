#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RUNTIME_DIR="${ROOT_DIR}/artifacts/dev"
PID_DIR="${RUNTIME_DIR}/pids"
LOG_DIR="${RUNTIME_DIR}/logs"

DB_CONTAINER="${LEXIQUEST_DB_CONTAINER:-lexiquest-dev-mssql}"
DB_IMAGE="${LEXIQUEST_DB_IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"
DB_PORT="${LEXIQUEST_DB_PORT:-14335}"
DB_NAME="${LEXIQUEST_DB_NAME:-LexiQuest}"
DB_PASSWORD="${LEXIQUEST_DB_PASSWORD:-LexiQuest_Dev_2026_Strong!42}"

API_PORT="${LEXIQUEST_API_PORT:-5083}"
WEB_PORT="${LEXIQUEST_WEB_PORT:-5300}"
API_URL="http://localhost:${API_PORT}"
WEB_URL="http://localhost:${WEB_PORT}"

CONNECTION_STRING="${LEXIQUEST_CONNECTION_STRING:-Server=127.0.0.1,${DB_PORT};Database=${DB_NAME};User Id=sa;Password=${DB_PASSWORD};MultipleActiveResultSets=true;TrustServerCertificate=True}"

mkdir -p "${PID_DIR}" "${LOG_DIR}"

log() {
    printf '[LexiQuest] %s\n' "$*"
}

die() {
    printf '[LexiQuest] ERROR: %s\n' "$*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || die "Chybí příkaz '$1'."
}

pid_is_running() {
    local pid="${1:-}"
    [[ -n "${pid}" ]] && kill -0 "${pid}" >/dev/null 2>&1
}

read_pid() {
    local pid_file="$1"
    [[ -f "${pid_file}" ]] || return 1
    tr -d '[:space:]' < "${pid_file}"
}

port_is_listening() {
    local port="$1"
    ss -ltn | awk '{print $4}' | grep -Eq "(:|\\])${port}$"
}

stop_process_group() {
    local pid="$1"

    if ! pid_is_running "${pid}"; then
        return 0
    fi

    kill -TERM -- "-${pid}" >/dev/null 2>&1 || kill -TERM "${pid}" >/dev/null 2>&1 || true

    for _ in $(seq 1 20); do
        pid_is_running "${pid}" || return 0
        sleep 0.5
    done

    kill -KILL -- "-${pid}" >/dev/null 2>&1 || kill -KILL "${pid}" >/dev/null 2>&1 || true
}

wait_for_http() {
    local name="$1"
    local url="$2"
    local pid_file="$3"
    local attempts="${4:-120}"
    local pid=""

    if [[ -f "${pid_file}" ]]; then
        pid="$(read_pid "${pid_file}")"
    fi

    for _ in $(seq 1 "${attempts}"); do
        if curl -fsS "${url}" >/dev/null 2>&1; then
            log "${name} běží: ${url}"
            return 0
        fi

        if [[ -n "${pid}" ]] && ! pid_is_running "${pid}"; then
            log "${name} skončil dřív, než začal odpovídat."
            tail -80 "${LOG_DIR}/${name}.log" 2>/dev/null || true
            exit 1
        fi

        sleep 1
    done

    log "${name} neodpověděl včas: ${url}"
    tail -120 "${LOG_DIR}/${name}.log" 2>/dev/null || true
    exit 1
}

ensure_port_available() {
    local name="$1"
    local port="$2"

    if port_is_listening "${port}"; then
        die "Port ${port} pro ${name} je už obsazený. Uvolni ho, nebo spusť s jiným portem přes LEXIQUEST_${name^^}_PORT."
    fi
}

start_mssql() {
    require_command docker

    if docker ps -a --format '{{.Names}}' | grep -Fxq "${DB_CONTAINER}"; then
        if docker ps --format '{{.Names}}' | grep -Fxq "${DB_CONTAINER}"; then
            log "MSSQL container už běží: ${DB_CONTAINER}"
        else
            log "Startuji existující MSSQL container: ${DB_CONTAINER}"
            docker start "${DB_CONTAINER}" >/dev/null
        fi
    else
        log "Vytvářím MSSQL container: ${DB_CONTAINER}"
        docker run -d \
            --name "${DB_CONTAINER}" \
            -e ACCEPT_EULA=Y \
            -e MSSQL_SA_PASSWORD="${DB_PASSWORD}" \
            -e MSSQL_PID=Developer \
            -p "127.0.0.1:${DB_PORT}:1433" \
            -v "${DB_CONTAINER}-data:/var/opt/mssql" \
            "${DB_IMAGE}" >/dev/null
    fi

    wait_for_mssql
}

wait_for_mssql() {
    log "Čekám na MSSQL na 127.0.0.1:${DB_PORT}..."

    for _ in $(seq 1 90); do
        if docker exec -e MSSQL_SA_PASSWORD="${DB_PASSWORD}" "${DB_CONTAINER}" bash -lc '
            sqlcmd_path=""
            if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
                sqlcmd_path=/opt/mssql-tools18/bin/sqlcmd
            elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
                sqlcmd_path=/opt/mssql-tools/bin/sqlcmd
            fi

            if [[ -n "${sqlcmd_path}" ]]; then
                "${sqlcmd_path}" -C -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -Q "SELECT 1" >/dev/null
            else
                exit 2
            fi
        ' >/dev/null 2>&1; then
            log "MSSQL je připravený."
            return 0
        fi

        if docker logs "${DB_CONTAINER}" 2>&1 | grep -q "SQL Server is now ready for client connections"; then
            if timeout 1 bash -c "</dev/tcp/127.0.0.1/${DB_PORT}" >/dev/null 2>&1; then
                log "MSSQL port odpovídá."
                return 0
            fi
        fi

        sleep 2
    done

    docker logs --tail 120 "${DB_CONTAINER}" >&2 || true
    die "MSSQL container nezačal odpovídat včas."
}

start_dotnet_app() {
    local name="$1"
    local project="$2"
    local port="$3"
    local health_url="$4"
    local pid_file="${PID_DIR}/${name}.pid"
    local log_file="${LOG_DIR}/${name}.log"

    if [[ -f "${pid_file}" ]]; then
        local pid
        pid="$(read_pid "${pid_file}")"
        if pid_is_running "${pid}" && curl -fsS "${health_url}" >/dev/null 2>&1; then
            log "${name} už běží: ${health_url}"
            return 0
        fi

        if pid_is_running "${pid}"; then
            log "Zastavuji starý ${name} proces (${pid}), který není zdravý..."
            stop_process_group "${pid}"
        fi

        rm -f "${pid_file}"
    fi

    ensure_port_available "${name}" "${port}"

    log "Startuji ${name}..."
    : > "${log_file}"

    if [[ "${name}" == "api" ]]; then
        setsid env \
            ASPNETCORE_ENVIRONMENT=Development \
            ASPNETCORE_URLS="${API_URL}" \
            ConnectionStrings__DefaultConnection="${CONNECTION_STRING}" \
            JwtSettings__SecretKey="LexiQuest-Dev-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!" \
            BlazorClient__Url="${WEB_URL}" \
            EmailSettings__Host="127.0.0.1" \
            EmailSettings__Port="2525" \
            EmailSettings__UseSsl="false" \
            EmailSettings__Username="" \
            EmailSettings__Password="" \
            EmailSettings__FromEmail="noreply@lexiquest.local" \
            EmailSettings__FromName="LexiQuest" \
            EmailSettings__BaseUrl="${WEB_URL}" \
            StripeSettings__ApiKey="sk_test_dev" \
            StripeSettings__WebhookSecret="whsec_dev" \
            dotnet run --project "${project}" --no-launch-profile \
            > "${log_file}" 2>&1 < /dev/null &
    else
        setsid env \
            ASPNETCORE_ENVIRONMENT=E2E \
            ASPNETCORE_URLS="${WEB_URL}" \
            ApiBaseUrl="${API_URL}" \
            dotnet run --project "${project}" --no-launch-profile \
            > "${log_file}" 2>&1 < /dev/null &
    fi

    echo "$!" > "${pid_file}"
    wait_for_http "${name}" "${health_url}" "${pid_file}"
}

require_command dotnet
require_command curl
require_command ss

start_mssql
start_dotnet_app "api" "${ROOT_DIR}/src/LexiQuest.Api/LexiQuest.Api.csproj" "${API_PORT}" "${API_URL}/health/ready"
start_dotnet_app "web" "${ROOT_DIR}/src/LexiQuest.Web/LexiQuest.Web.csproj" "${WEB_PORT}" "${WEB_URL}/"

cat <<EOF

LexiQuest běží:
  Web: ${WEB_URL}
  API: ${API_URL}
  DB:  127.0.0.1:${DB_PORT} (${DB_CONTAINER})

Logy:
  ${LOG_DIR}/api.log
  ${LOG_DIR}/web.log

Vypnutí:
  ${ROOT_DIR}/scripts/dev-stop.sh
EOF
