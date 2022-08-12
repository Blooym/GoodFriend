/* eslint-disable no-console */
import ratelimit from 'express-rate-limit';
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';
import router from './Routes/v1';
import logger from './Utils/Logger';

// Inject .env variables into process.env
require('dotenv').config();

// Create the express app & set the port to listen on
const app = express().disable('x-powered-by');
const port = process.env.PORT || 8000;

// Setup the ratelimiter for the API
const limiter = ratelimit({
  windowMs: 60 * 5000,
  max: 6,
  message: 'Too many requests from this IP, try again later',
});

// Middlewares and routes
app.use(limiter);
app.use(logger);
app.use('/v1', router);
app.get('/', (_, res) => res.sendStatus(200));

// If a keyfile & certfile have been specified, use https
const { KEYFILE, CERTFILE, NODE_ENV } = process.env;
if (KEYFILE && CERTFILE) {
  const key = fs.readFileSync(KEYFILE);
  const cert = fs.readFileSync(CERTFILE);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { console.log(`[SECURE] Listening on port HTTPS:${port}`); });
}

// If no key/cert files have been specified and this is prod, assume the local machine handles SSL.
else if (NODE_ENV === 'production') { https.createServer(app).listen(port, () => { console.log(`[SECURE] Listening on port HTTPS:${port}`); }); }

// Otherwise, just run as a regular HTTP server.
else { http.createServer(app).listen(port, () => { console.log(`[INSECURE] Listening on port HTTP:${port}`); }); }
