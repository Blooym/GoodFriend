import { rateLimit } from 'express-rate-limit';

export default rateLimit({
  windowMs: 60 * 5000,
  max: 12,
  standardHeaders: true,
});
