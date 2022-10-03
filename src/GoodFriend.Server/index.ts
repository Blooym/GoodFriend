/* eslint-disable no-console */
import express from 'express';
import compression from 'compression';
import helmet from 'helmet';
import { PORT, TRUST_PROXY } from '@base/environment';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';
import RequireSessionIdentifier from '@middleware/RequireSessionIdentifier';
import PreventBannedUseragent from '@middleware/PreventBannedUseragent';
import MetricsCollector from '@middleware/MetricsCollector';

import globalRouter from '@routes/Global';
import v3Router from '@routes/v3';

const app = express()
  .use(helmet())
  .use(compression())
  .use(MetricsCollector)
  .use(Ratelimitter)
  .use(logger)
  .use(PreventBannedUseragent)
  .use('/', globalRouter)
  .use('/v3', RequireSessionIdentifier, v3Router)
  .use(errorLogger);

// if TRUST_PROXY is set, trust proxies.
if (TRUST_PROXY) app.set('trust proxy', true);

app.listen(PORT, () => { console.log(`[HTTP] Listening on port ${PORT}`); });
