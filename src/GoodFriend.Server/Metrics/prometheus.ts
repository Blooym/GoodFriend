import client from 'prom-client';

const register = new client.Registry();

export const totalSSEClients = new client.Gauge({
  name: 'total_sse_clients',
  help: 'Total number of clients connected to the server for SSE events',
  registers: [register],
});

export const totalSSEStateEvents = new client.Counter({
  name: 'total_sse_state_events',
  help: 'Total number of state events sent to clients',
  registers: [register],
  labelNames: ['event', 'homeworld', 'territory'],
});

export const totalSSESessionTime = new client.Counter({
  name: 'total_sse_session_time',
  help: 'Total time spent by users in the game',
  registers: [register],
});

export const totalRequests = new client.Gauge({
  name: 'total_requests',
  help: 'Total number of requests received by the server',
  registers: [register],
  labelNames: ['method', 'route', 'status_code'],
});

export const averageResponseTime = new client.Histogram({
  name: 'average_response_time',
  help: 'Average response time of requests',
  registers: [register],
  labelNames: ['method', 'route', 'status_code'],
});

client.collectDefaultMetrics({ register });

export default register;
