import winston from 'winston';
import expressWinston from 'express-winston';

/**
 * Logs all requests to the server.
 */
export const logger = expressWinston.logger({
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

/**
 * Logs all errors thrown by the application.
 */
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
