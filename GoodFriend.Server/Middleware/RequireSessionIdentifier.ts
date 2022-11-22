import { NextFunction, Request, Response } from 'express';
import { isValidGUID } from '@utils/Validators';
import IsAuthenticated from '@utils/Common';

/**
 * Requires a X-Session-Identifier header to be present in the request, or an authorization token.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  const identifier = req.header('x-session-identifier');
  const authorization = req.header('authorization');

  if (IsAuthenticated(authorization)) {
    return next();
  }

  if (!identifier || !isValidGUID(identifier)) {
    res.status(400).send('Missing or invalid X-Session-Identifier header; please provide a valid GUID.');
    return null;
  }

  return next();
};
