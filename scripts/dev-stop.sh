#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RUNTIME_DIR="${ROOT_DIR}/artifacts/dev"
PID_DIR="${RUNTIME_DIR}/pids"
LOG_DIR="${RUNTIME_DIR}/logs"

DB_CONTAINER="${LEXIQUEST_DB_CONTAINER:-lexiquest-dev-mssql}"
KEEP_DB=false

for arg in "$@"; do
    case "${arg}" in
        --keep-db)
            KEEP_DB=true
            ;;
        -h|--help)
            cat <<EOF
Použití:
  scripts/dev-stop.sh            Vypne web, API a Docker DB container.
  scripts/dev-stop.sh --keep-db  Vypne jen web a API, DB nechá běžet.
EOF
            exit 0
            ;;
        *)
            printf '[LexiQuest] ERROR: Neznámý argument: %s\n' "${arg}" >&2
            exit 1
            ;;
    esac
done

log() {
    printf '[LexiQuest] %s\n' "$*"
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

stop_process_group() {
    local name="$1"
    local pid_file="$2"

    if [[ ! -f "${pid_file}" ]]; then
        log "${name}: PID soubor neexistuje, přeskakuji."
        return 0
    fi

    local pid
    pid="$(read_pid "${pid_file}")"

    if ! pid_is_running "${pid}"; then
        log "${name}: proces ${pid} už neběží."
        rm -f "${pid_file}"
        return 0
    fi

    log "Vypínám ${name} (${pid})..."
    kill -TERM -- "-${pid}" >/dev/null 2>&1 || kill -TERM "${pid}" >/dev/null 2>&1 || true

    for _ in $(seq 1 20); do
        if ! pid_is_running "${pid}"; then
            rm -f "${pid_file}"
            return 0
        fi

        sleep 0.5
    done

    log "${name} nereaguje na SIGTERM, posílám SIGKILL..."
    kill -KILL -- "-${pid}" >/dev/null 2>&1 || kill -KILL "${pid}" >/dev/null 2>&1 || true
    rm -f "${pid_file}"
}

stop_database() {
    if "${KEEP_DB}"; then
        log "DB nechávám běžet (--keep-db)."
        return 0
    fi

    if ! command -v docker >/dev/null 2>&1; then
        log "Docker není dostupný, DB nepůjde vypnout."
        return 0
    fi

    if docker ps --format '{{.Names}}' | grep -Fxq "${DB_CONTAINER}"; then
        log "Vypínám MSSQL container: ${DB_CONTAINER}"
        docker stop "${DB_CONTAINER}" >/dev/null
    else
        log "MSSQL container neběží: ${DB_CONTAINER}"
    fi
}

stop_process_group "web" "${PID_DIR}/web.pid"
stop_process_group "api" "${PID_DIR}/api.pid"
stop_database

log "Hotovo. Logy zůstaly v ${LOG_DIR}."
