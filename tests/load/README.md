# Load Testing s k6

## Prerekvizity
- [k6](https://k6.io/docs/get-started/installation/)

## Spuštění

### Login + Game flow (100 concurrent users)
```bash
k6 run --env API_URL=https://localhost:5001 tests/load/k6-config.js
```

### Dashboard load (1000 users)
```bash
k6 run --env API_URL=https://localhost:5001 tests/load/dashboard-load.js
```

### Multiplayer load (50 concurrent matches)
```bash
k6 run --env API_URL=https://localhost:5001 tests/load/multiplayer-load.js
```

## Metriky
- p50/p95/p99 response times
- Error rate
- Throughput (req/s)

## Threshold
- p95 < 500ms
- p99 < 1000ms
- Error rate < 1%
