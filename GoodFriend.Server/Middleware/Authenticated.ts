import { NextFunction, Request, Response } from 'express';
import { AUTHENTICATED_HEADERS } from '@common/headers';
import IsAuthenticated from '@utils/Common';

/**
 * Sets the given endpoint as authenticate, requiring a valid token to be passed in the request.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  res.header(AUTHENTICATED_HEADERS);
  if (IsAuthenticated(req.header('authorization'))) next();
  else res.sendStatus(401);
};
