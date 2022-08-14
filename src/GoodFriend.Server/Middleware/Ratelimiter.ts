import { rateLimit } from 'express-rate-limit';

export default rateLimit({
  windowMs: 60 * 5000,
  max: 10,
  message: 'Too many requests from this IP, try again later',
});
