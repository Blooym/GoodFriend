import winston from 'winston';
import expressWinston from 'express-winston';
import { LOGGING_KEEP_FOR_DAYS, LOG_PATH } from '@common/environment';
import 'winston-daily-rotate-file';

/**
 * Logs all requests to the server in a daily-rotated file and to the console.
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
      filename: `${LOG_PATH}/log-%DATE%`,
      extension: '.log',
      maxFiles: LOGGING_KEEP_FOR_DAYS,
      format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.printf((info) => `[${new Date(info.timestamp).toUTCString()}] ${info.level.toUpperCase()}: ${info.meta.res.statusCode} ${info.meta.responseTime}ms ${info.message} [Agent: ${info.meta.req.headers['user-agent']}] [Session: ${info.meta.req.headers['x-session-identifier'] ?? 'none'}]`),
      ),
    }),
  ],
});

/**
 * Logs all errors thrown by the application in a daily-rotated file and to the console.
 */
export const errorLogger = expressWinston.errorLogger({
  level: 'error',
  transports: [
    new winston.transports.Console({
      format:
        winston.format.combine(
          winston.format.colorize(),
          winston.format.timestamp(),
          winston.format.json(),
        ),
    }),
    new winston.transports.DailyRotateFile({
      filename: `${LOG_PATH}/errors-%DATE%`,
      extension: '.log',
      maxFiles: LOGGING_KEEP_FOR_DAYS,
      format: winston.format.combine(
        winston.format.timestamp(),
        winston.format.json(),
      ),
    }),
  ],
});
