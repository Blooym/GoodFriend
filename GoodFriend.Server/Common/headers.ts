/**
 * The headers to be sent with authenticated requests.
 */
export const AUTHENTICATED_HEADERS = {
  'Cache-Control': 'no-cache',
  Pragma: 'no-cache',
};

/**
 * The headers to send with SSE responses.
 */
export const SSE_HEADERS = {
  'Content-Type': 'text/event-stream',
  'Cache-Control': 'no-cache',
  Connection: 'keep-alive',
  'X-Accel-Buffering': 'no',
};
