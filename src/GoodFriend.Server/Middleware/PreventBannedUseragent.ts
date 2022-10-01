import { NextFunction, Request, Response } from 'express';
import { BANNED_USERAGENTS } from '@base/environment';

/**
 * Prevents banned user agents from accessing the API.
 * @param req The request object.
 * @param res The response object.
 * @param next The next middleware function to call.
 */
export default (req: Request, res: Response, next: NextFunction) => {
  if (BANNED_USERAGENTS.includes(req.headers['user-agent'] as string))
  { res.status(403).send('Forbidden access: agent is banned (typically due to abuse or outdated version).'); }

  else next();
};
