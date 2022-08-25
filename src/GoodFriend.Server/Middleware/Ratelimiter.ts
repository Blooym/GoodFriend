import { rateLimit } from 'express-rate-limit';

/**
 * Limits the number of requests per IP address.
 */
export default rateLimit({
  windowMs: 60 * 5000,
  max: 12,
  standardHeaders: true,
  keyGenerator: (req) => {
    const ip = req.headers['x-forwarded-for'] || req.ip;
    return ip as string;
  },
});
