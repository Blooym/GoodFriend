import winston from 'winston';
import expressWinston from 'express-winston';
import 'winston-daily-rotate-file';
import 'dotenv/config';

const LOG_PATH = 'logs';
const LOG_DAYS_TO_KEEP = process.env.LOG_DAYS_TO_KEEP || '7d';

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
      filename: `${LOG_PATH}/events-%DATE%`,
      extension: '.log',
      maxFiles: LOG_DAYS_TO_KEEP,
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
      filename: `${LOG_PATH}/errors-%DATE%`,
      extension: '.log',
      maxFiles: LOG_DAYS_TO_KEEP,
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
