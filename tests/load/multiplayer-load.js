import http from 'k6/http';
import { check, sleep } from 'k6';
import ws from 'k6/ws';

const BASE_URL = __ENV.API_URL || 'https://localhost:5001';

export const options = {
    scenarios: {
        multiplayer_matches: {
            executor: 'constant-vus',
            vus: 100,  // 50 concurrent matches
            duration: '3m',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<1000'],
        http_req_failed: ['rate<0.05'],
    },
};

export default function () {
    // Login first
    const loginRes = http.post(`${BASE_URL}/api/v1/users/login`, JSON.stringify({
        email: `loadtest${__VU}@test.com`,
        password: 'TestPass123!'
    }), { headers: { 'Content-Type': 'application/json' } });

    if (loginRes.status !== 200) return;

    const token = JSON.parse(loginRes.body).token;
    const headers = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
    };

    // Join matchmaking queue
    const queueRes = http.post(`${BASE_URL}/api/v1/multiplayer/queue`, null, { headers });
    check(queueRes, { 'joined queue': (r) => r.status === 200 || r.status === 201 });

    sleep(2);

    // Cancel queue
    http.del(`${BASE_URL}/api/v1/multiplayer/queue`, null, { headers });
    sleep(1);
}
