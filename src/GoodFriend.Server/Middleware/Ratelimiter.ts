import { rateLimit } from 'express-rate-limit';

/**
 * Limits the number of requests per IP address.
 */
export default rateLimit({
  windowMs: 60 * 1000,
  max: Number(process.env.REQS_PER_MIN) || 6,
  standardHeaders: true,
});
