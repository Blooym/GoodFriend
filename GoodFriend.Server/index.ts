/* eslint-disable no-console */
import express from 'express';
import compression from 'compression';
import { PORT, TRUST_PROXY } from '@common/environment';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';
import RequireSessionIdentifier from '@middleware/RequireSessionIdentifier';
import MetricsCollector from '@services/Prometheus/Middleware';

import globalRouter from '@routes/Global';
import v4Router from '@routes/v4';
import Helmet from '@middleware/Helmet';

const app = express()
  .use(Helmet)
  .use(compression())
  .use(Ratelimitter)
  .use(MetricsCollector)
  .use(logger)
  .use('/', globalRouter)
  .use('/v4', RequireSessionIdentifier, v4Router)
  .use(errorLogger);

if (TRUST_PROXY) app.set('trust proxy', true);

app.listen(PORT, () => { console.log(`[HTTP] Listening on port ${PORT}`); });
