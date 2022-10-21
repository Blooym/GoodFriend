import { NextFunction, Request, Response } from 'express';

import { totalRequests } from '@metrics/prometheus';

export default (req: Request, res: Response, next: NextFunction) => {
  totalRequests.inc({ method: req.method, route: req.path, status_code: res.statusCode }, 1);
  next();
};
