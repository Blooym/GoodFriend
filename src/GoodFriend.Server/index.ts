/* eslint-disable no-console */
import express from 'express';
import http from 'http';
import compression from 'compression';
import helmet from 'helmet';
import 'dotenv/config';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';

import globalRouter from '@routes/Global';
import v2Router from '@routes/v2';

const port = process.env.PORT || 8000;

const app = express()
  .use(helmet())
  .use(compression())
  .use(Ratelimitter)
  .use(logger)
  .use('/', globalRouter)
  .use('/v2', v2Router)
  .get('*', (req, res) => res.sendStatus(404))
  .use(errorLogger);

// if TRUST_PROXY is set, trust proxies.
if (process.env.TRUST_PROXY) app.set('trust proxy', true);

http.createServer(app).listen(port, () => { console.log(`[HTTP] Listening on port ${port}`); });
