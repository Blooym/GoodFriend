import ratelimit from 'express-rate-limit';
import express from 'express';
import fs from 'fs';
import https from 'https';
import http from 'http';
import winston from 'winston';
import expresswinston from 'express-winston';
import router from './Routes/v1';
import { keyFile, certFile } from './SSLConfig';

// Create the express app & set the port to listen on
const app = express().disable('x-powered-by');
const port = process.env.PORT || 3000;
const log = winston.createLogger(
  {
    level: 'info',
    transports: [new winston.transports.Console()],
    format: winston.format.combine(
      winston.format.colorize(),
      winston.format.printf((info) => `${info.level}: ${info.message}`),
    ),
  },
);

// Setup the ratelimiter for the API
const limiter = ratelimit({
  windowMs: 60 * 5000,
  max: 6,
  message: 'Too many requests from this IP, try again later',
});

// Middlewares and routes
app.use(limiter);
app.use(express.urlencoded({ extended: false }));
app.use(expresswinston.logger({
  transports: [
    new winston.transports.Console(),
  ],
  format: winston.format.combine(
    winston.format.colorize(),
    winston.format.printf((info) => `${info.level}: ${info.message}`),
  ),
  colorize: true,
  meta: false,
  expressFormat: true,
}));

app.use('/v1', router);
app.get('/', (req, res) => res.sendStatus(200));

// Create server & start listening
if (process.env.NODE_ENV === 'production' || process.env.SECURE === 'true') {
  const key = fs.readFileSync(keyFile);
  const cert = fs.readFileSync(certFile);
  const credentials = { key, cert };
  https.createServer(credentials, app).listen(port, () => { log.info(`HTTPS Server listening on port ${port}`); });
} else http.createServer(app).listen(port, () => { log.info(`HTTP Server listening on port ${port}`); });
