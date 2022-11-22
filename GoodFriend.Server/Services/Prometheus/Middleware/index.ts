import { NextFunction, Request, Response } from 'express';

import { totalRequests } from '@services/Prometheus';

/**
 * Handles collecting metrics about requests.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  totalRequests.inc({ method: req.method, route: req.path, status_code: res.statusCode }, 1);
  next();
};
