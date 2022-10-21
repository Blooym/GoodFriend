import { NextFunction, Request, Response } from 'express';
import { AUTH_TOKEN } from '@base/environment';

const AUTHENTICATED_HEADERS = {
  'Cache-Control': 'no-cache, no-store, must-revalidate',
  Pragma: 'no-cache',
};

/**
 * Sets the given endpoint as authenticated.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  res.header(AUTHENTICATED_HEADERS);

  const authorization = req.headers.authorization?.replace('Bearer ', '');

  if (authorization === AUTH_TOKEN) next();
  else res.sendStatus(401);
};
