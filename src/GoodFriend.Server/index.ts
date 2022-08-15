/* eslint-disable no-console */
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';

import Ratelimitter from '@middleware/Ratelimiter';
import { logger, errorLogger } from '@middleware/Logger';
import HeaderSettings from '@middleware/HeaderSettings';

import router from '@routes/v1';
import ErrorHandler from '@middleware/ErrorHandler';

require('dotenv').config();

const port = process.env.APP_PORT || 8000;
const { SSL_KEYFILE, SSL_CERTFILE, NODE_ENV } = process.env;

const app = express()
  .use(HeaderSettings)
  .use(Ratelimitter)
  .use(logger)
  .get('/', (req, res) => res.sendStatus(200))
  .use('/v1', router)
  .get('*', (req, res) => res.sendStatus(404))
  .use(errorLogger)
  .use(ErrorHandler);

// If we've got a SSL files to use, start a HTTPs server with them.
if (SSL_KEYFILE && SSL_CERTFILE) {
  const key = fs.readFileSync(SSL_KEYFILE);
  const cert = fs.readFileSync(SSL_CERTFILE);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); });
}

// If we're in production with no SSL files, start a HTTPs and let the host handle it.
else if (NODE_ENV === 'production') { https.createServer(app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); }); }

// Otherwise, start a HTTP server.
else { http.createServer(app).listen(port, () => { console.log(`[HTTP] Listening on port ${port}`); }); }
