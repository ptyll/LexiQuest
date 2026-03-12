import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';

const BASE_URL = __ENV.API_URL || 'https://localhost:5001';
const errorRate = new Rate('errors');
const loginDuration = new Trend('login_duration');

export const options = {
    scenarios: {
        concurrent_users: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 50 },
                { duration: '1m', target: 100 },
                { duration: '30s', target: 100 },
                { duration: '30s', target: 0 },
            ],
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500', 'p(99)<1000'],
        errors: ['rate<0.01'],
    },
};

function login() {
    const res = http.post(`${BASE_URL}/api/v1/users/login`, JSON.stringify({
        email: `loadtest${__VU}@test.com`,
        password: 'TestPass123!'
    }), { headers: { 'Content-Type': 'application/json' } });

    loginDuration.add(res.timings.duration);
    check(res, { 'login successful': (r) => r.status === 200 });

    if (res.status === 200) {
        return JSON.parse(res.body).token;
    }
    return null;
}

export default function () {
    group('Login and Play', () => {
        const token = login();
        if (!token) return;

        const headers = {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
        };

        // Start game
        const startRes = http.post(`${BASE_URL}/api/v1/game/start`, JSON.stringify({
            mode: 'Training',
            difficulty: 'Beginner'
        }), { headers });

        check(startRes, { 'game started': (r) => r.status === 200 || r.status === 201 });
        errorRate.add(startRes.status >= 400);

        sleep(1);
    });

    sleep(1);
}
