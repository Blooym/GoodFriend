import { NextFunction, Request, Response } from 'express';
import { AUTH_TOKEN } from '@base/environment';

/**
 * Requires a X-Session0Identifier header to be present in the request.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  const uuid = req.header('x-session-identifier');
  const authorization = req.header('authorization');

  if (authorization?.replace('Bearer ', '') === AUTH_TOKEN) { next(); return; }

  if (!uuid || !uuid.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)) {
    res.status(400).send('Invalid session identifier');
    return;
  }

  next();
};
