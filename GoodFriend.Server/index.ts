/* eslint-disable no-console */
import express from 'express';
import compression from 'compression';
import helmet from 'helmet';
import { PORT, TRUST_PROXY } from '@base/environment';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';
import RequireSessionIdentifier from '@middleware/RequireSessionIdentifier';
import MetricsCollector from '@middleware/MetricsCollector';

import globalRouter from '@routes/Global';
import v3Router from '@routes/v3';
import v4Router from '@routes/v4';

const app = express()
  .use(helmet())
  .use(compression())
  .use(Ratelimitter)
  .use(MetricsCollector)
  .use(logger)
  .use('/', globalRouter)
  .use('/v3', v3Router)
  .use('/v4', RequireSessionIdentifier, v4Router)
  .use(errorLogger);

console.log("test");

if (TRUST_PROXY) app.set('trust proxy', true);

app.listen(PORT, () => { console.log(`[HTTP] Listening on port ${PORT}`); });
