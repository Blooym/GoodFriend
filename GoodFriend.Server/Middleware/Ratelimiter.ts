import { rateLimit } from 'express-rate-limit';
import { RATELIMIT_RESET_MINS, REQUESTS_PER_RESET } from '@base/environment';

/**
 * Limits the number of requests per IP address.
 */
export default rateLimit({
  windowMs: Number(RATELIMIT_RESET_MINS) * 60 * 1000 || 1 * 60 * 1000,
  max: Number(REQUESTS_PER_RESET) || 30,
  standardHeaders: true,
});
