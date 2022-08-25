import { rateLimit } from 'express-rate-limit';

/**
 * Limits the number of requests per IP address.
 */
export default rateLimit({
  windowMs: 60 * 5000,
  max: 12,
  standardHeaders: true,
});
