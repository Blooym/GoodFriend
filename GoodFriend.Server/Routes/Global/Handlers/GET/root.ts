import { Request, Response } from 'express';

/**
 * Serves a 200 OK response for the / route.
 * @param req The request object.
 * @param res The response object.
 */
export default async (req: Request, res: Response) => {
  res.sendStatus(200);
};
