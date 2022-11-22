import { rateLimit } from 'express-rate-limit';
import { RATELIMIT_RESET_MINS, RATELIMIT_REQUESTS_PER_RESET, RATELIMIT_IGNORE_AUTHENTICATED } from '@common/environment';
import IsAuthenticated from '@utils/Common';

/**
 * Limits the number of requests per IP address as configured by the environment.
 */
export default rateLimit({
  windowMs: Number(RATELIMIT_RESET_MINS) * 60 * 1000 || 1 * 60 * 1000,
  max: Number(RATELIMIT_REQUESTS_PER_RESET) || 30,
  standardHeaders: true,
  legacyHeaders: true,
  skip: (req) => {
    if (RATELIMIT_IGNORE_AUTHENTICATED) {
      return IsAuthenticated(req.headers.authorization);
    }
    return false;
  },
});
