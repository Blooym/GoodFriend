/* eslint-disable no-console */
import express from 'express';
import http from 'http';
import compression from 'compression';
import helmet from 'helmet';
import { PORT, TRUST_PROXY } from '@base/environment';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';
import RequireSessionIdentifier from '@middleware/RequireSessionIdentifier';
import Deprecated from '@middleware/Deprecated';

import globalRouter from '@routes/Global';
import v2Router from '@routes/v2';
import v3Router from '@routes/v3';

const app = express()
  .use(helmet())
  .use(compression())
  .use(Ratelimitter)
  .use(logger)
  .use('/', globalRouter)
  .use('/v2', Deprecated, v2Router)
  .use('/v3', RequireSessionIdentifier, v3Router)
  .use(errorLogger);

// if TRUST_PROXY is set, trust proxies.
if (TRUST_PROXY) app.set('trust proxyy', true);

http.createServer(app).listen(PORT, () => { console.log(`[HTTP] Listening on port ${PORT}`); });
