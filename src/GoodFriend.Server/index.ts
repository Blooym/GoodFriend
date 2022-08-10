/* eslint-disable no-console */
import ratelimit from 'express-rate-limit';
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';
import router from './Routes/v1';
import logger from './logger';

// Inject .env variables into process.env
require('dotenv').config();

// Create the express app & set the port to listen on
const app = express().disable('x-powered-by');
const port = process.env.PORT || 3000;

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

// Create the server
if (process.env.NODE_ENV === 'production' && process.env.KEYFILE && process.env.CERTFILE) {
  const key = fs.readFileSync(process.env.KEYFILE);
  const cert = fs.readFileSync(process.env.CERTFILE);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { console.log(`Listening on port HTTPS:${port}`); });
}
else if (process.env.NODE_ENV === 'production') { https.createServer(app).listen(port, () => { console.log(`Listening on port HTTPS:${port}`); }); }
else { http.createServer(app).listen(port, () => { console.log(`Listening on port HTTP:${port}`); }); }
