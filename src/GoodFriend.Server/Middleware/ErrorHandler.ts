import { NextFunction, Request, Response } from 'express';

export default (err: Error, req: Request, res: Response, next: NextFunction) => {
  res.sendStatus(500);
  next(err);
};
