/* eslint-disable no-console */
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';
import compression from 'compression';
import helmet from 'helmet';
import 'dotenv/config';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';

import globalRouter from '@routes/Global';
import v2Router from '@routes/v2';

const port = process.env.PORT || 8000;
const { SSL_KEYFILE, SSL_CERTFILE } = process.env;

const app = express()
  .use(helmet())
  .use(compression())
  .use(Ratelimitter)
  .use(logger)
  .use('/', globalRouter)
  .use('/v2', v2Router)
  .get('*', (req, res) => res.sendStatus(404))
  .use(errorLogger);

// If there is an SSL keyfile and certfile set, use HTTPS.
if (SSL_KEYFILE && SSL_CERTFILE) {
  const key = fs.readFileSync(SSL_KEYFILE);
  const cert = fs.readFileSync(SSL_CERTFILE);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); });
}

// Otherwise, start an insecure server and allow the SSL connection to be handled elsewhere.
else { http.createServer(app).listen(port, () => { console.log(`[HTTP] Listening on port ${port}`); }); }
