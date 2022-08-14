import winston from 'winston';
import expresswinston from 'express-winston';

export default expresswinston.logger({
  ignoreRoute: (req, res) => req.path.includes('/clients') || req.path.includes('/events/') || res.statusCode === 404,
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
