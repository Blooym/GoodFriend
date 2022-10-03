import { NextFunction, Request, Response } from 'express';

import { averageResponseTime, totalRequests } from '@metrics/prometheus';

export default (req: Request, res: Response, next: NextFunction) => {
  const stopTimer = averageResponseTime.startTimer({
    method: req.method,
    route: req.path,
    status_code: res.statusCode,
  });

  res.on('finish', () => {
    stopTimer();
    totalRequests.inc({ method: req.method, route: req.path, status_code: res.statusCode }, 1);
  });
  next();
};
