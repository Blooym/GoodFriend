import winston from 'winston';
import expresswinston from 'express-winston';

export default expresswinston.logger({
  transports: [
    new winston.transports.Console(),
  ],
  format: winston.format.combine(
    winston.format.colorize(),
    winston.format.printf((info) => `${info.level}: ${info.message}`),
  ),
  colorize: true,
  meta: true,
  expressFormat: true,
});
