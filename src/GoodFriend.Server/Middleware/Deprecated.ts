import { NextFunction, Request, Response } from 'express';

/**
 * Sets the given endpoint as deprecated.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  res.header('Deprecated', 'true');
  res.header('Warning', '299 - "Resource is deprecated"');
  next();
};
