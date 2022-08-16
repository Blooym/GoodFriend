import { NextFunction, Request, Response } from 'express';

/**
 * Handles all errors thrown by the application.
 * @param err The error thrown by the application.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (err: Error, req: Request, res: Response, next: NextFunction) => {
  res.sendStatus(500);
  next(err);
};
