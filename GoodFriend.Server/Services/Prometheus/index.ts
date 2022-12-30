import client from 'prom-client';

const register = new client.Registry();

/**
 * The current number of SSE clients connected.
 */
export const currentSSEClients = new client.Gauge({
  name: 'current_sse_clients',
  help: 'The current number of SSE clients connected to the server.',
  registers: [register],
});

/**
 * The all time total number of SSE clients connected.
 */
export const allTimeSSEClients = new client.Counter({
  name: 'all_time_sse_clients',
  help: 'The all time total number of SSE clients connected to the server.',
  registers: [register],
});

/**
 * The total time spent by SSE clients connected to the server.
 */
export const totalSSESessionTime = new client.Counter({
  name: 'total_sse_session_time',
  help: 'The total time in milliseconds that SSE clients have been connected to the server, updated when a connection is closed.',
  registers: [register],
});

/**
 * Total number fo loginstate updates recieved from clients.
 */
export const totalLoginstateUpdatesReceived = new client.Counter({
  name: 'total_loginstate_updates_received',
  help: 'The total number of loginstate updates received from the server.',
  labelNames: ['type', 'territory', 'world', 'datacenter'],
  registers: [register],
});

/**
 * The total number of outbound SSE messages sent to clients.
 */
export const totalSSEMesssagesSent = new client.Counter({
  name: 'total_sse_messages_sent',
  help: 'The total number of outbound SSE messages sent to clients.',
  registers: [register],
});

/**
 * The total number of inbound requests recieved by the server.
 */
export const totalRequests = new client.Counter({
  name: 'total_requests',
  help: 'The total number of requests recieved by the server.',
  labelNames: ['method', 'route', 'status_code'],
  registers: [register],
});

// Collect default metrics.
client.collectDefaultMetrics({ register });

export default register;
