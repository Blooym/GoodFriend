/* eslint-disable no-console */
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';

import ratelimitter from '@middleware/Ratelimiter';
import logger from '@middleware/Logger';
import router from '@routes/v1';

require('dotenv').config();

const port = process.env.APP_PORT || 8000;
const { SSL_KEYFILE, SSL_CERTFILE, NODE_ENV } = process.env;

const app = express()
  .use(ratelimitter)
  .use(logger)
  .use(router)
  .use('/v1', router)
  .disable('x-powered-by')
  .get('/', (req, res) => res.sendStatus(200));

// IF we've got a SSL files to use, start a HTTPs server with them.
if (SSL_KEYFILE && SSL_CERTFILE) {
  const key = fs.readFileSync(SSL_KEYFILE);
  const cert = fs.readFileSync(SSL_CERTFILE);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); });
}

// If we're in production with no SSL files, start a HTTPs and let the host handle it.
else if (NODE_ENV === 'production') { https.createServer(app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); }); }

// Otherwise, start a HTTP server.
else { http.createServer(app).listen(port, () => { console.log(`[HTTPS] Listening on port ${port}`); }); }
