import winston from 'winston';
import expressWinston from 'express-winston';

export const logger = expressWinston.logger({
  ignoreRoute: (req) => req.path.includes('/clients') || req.path.includes('/events/'),
  level: 'info',
  transports: [
    new winston.transports.Console({
      format: winston.format.combine(
        winston.format.colorize(),
        winston.format.timestamp(),
        winston.format.printf((info) => `[${new Date(info.timestamp).toUTCString()}] ${info.level}: ${info.message} ${info.meta.res.statusCode} ${info.meta.responseTime}ms`),
      ),
    }),
    new winston.transports.File({
      filename: 'logs/event.log',
      format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.printf((info) => `[${new Date(info.timestamp).toUTCString()}] ${info.level}: ${info.message} ${info.meta.res.statusCode} ${info.meta.responseTime}ms [${info.meta.req.headers['user-agent']}]`),
      ),
    }),
  ],
});

export const errorLogger = expressWinston.errorLogger({
  level: 'error',
  transports: [
    new winston.transports.File({
      filename: 'logs/error.log',
    }),
  ],
  format: winston.format.combine(
    winston.format.colorize(),
    winston.format.json(),
  ),
});
