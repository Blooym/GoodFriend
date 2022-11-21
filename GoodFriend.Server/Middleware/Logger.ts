import winston from 'winston';
import expressWinston from 'express-winston';
import 'winston-daily-rotate-file';
import 'dotenv/config';

const LOG_PATH = 'logs';

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
        winston.format.printf((info) => `[${new Date(info.timestamp).toUTCString()}] ${info.level.toUpperCase()}: ${info.meta.res.statusCode} ${info.meta.responseTime}ms ${info.message}`),
      ),
    }),
    new winston.transports.DailyRotateFile({
      filename: `${LOG_PATH}/events-%DATE%`,
      extension: '.log',
      maxFiles: 7,
      format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.printf((info) => `[${new Date(info.timestamp).toUTCString()}] ${info.level.toUpperCase()}: ${info.meta.res.statusCode} ${info.meta.responseTime}ms ${info.message} [Agent: ${info.meta.req.headers['user-agent']}] [Session: ${info.meta.req.headers['x-session-identifier'] ?? 'none'}] [Authorization: ${info.meta.req.headers.authorization ?? 'none'}]`),
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
      maxFiles: 7,
    }),
  ],
  format: winston.format.combine(
    winston.format.colorize(),
    winston.format.timestamp(),
    winston.format.json(),
  ),
});
