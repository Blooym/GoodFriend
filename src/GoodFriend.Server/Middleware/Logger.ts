import winston from 'winston';
import expressWinston from 'express-winston';
import 'winston-daily-rotate-file';

const MAX_DAYS_KEEP = '7d';
const LOG_DIR = 'logs';

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
    new winston.transports.DailyRotateFile({
      filename: `${LOG_DIR}/event.log`,
      extension: '.log',
      maxFiles: MAX_DAYS_KEEP,
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
    new winston.transports.DailyRotateFile({
      filename: `${LOG_DIR}/error.log`,
      extension: '.log',
      maxFiles: MAX_DAYS_KEEP,
      format: winston.format.combine(
        winston.format.json(),
      ),
    }),
  ],
  format: winston.format.combine(
    winston.format.colorize(),
    winston.format.json(),
  ),
});
