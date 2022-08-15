import { NextFunction, Request, Response } from 'express';

const AUTHENTICATED_HEADERS = {
  'Cache-Control': 'no-store, no-cache, must-revalidate',
  Pragma: 'no-cache',
};

export default (req: Request, res: Response, next: NextFunction) => {
  if (process.env.DISABLE_AUTH_ENDPOINTS === 'true') { res.sendStatus(404); return; }

  res.header(AUTHENTICATED_HEADERS);

  const { AUTH_TOKEN } = process.env;
  const authorization = req.header('authorization');

  if (authorization && authorization === AUTH_TOKEN) next();
  else res.sendStatus(401);
};
