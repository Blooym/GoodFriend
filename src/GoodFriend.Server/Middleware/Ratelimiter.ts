import { rateLimit } from 'express-rate-limit';

/**
 * Limits the number of requests per IP address.
 */
export default rateLimit({
  windowMs: Number(process.env.RATELIMIT_RESET_MINS) * 60 * 1000 || 1 * 60 * 1000,
  max: Number(process.env.REQUESTS_PER_RESET) || 30,
  standardHeaders: true,
});
