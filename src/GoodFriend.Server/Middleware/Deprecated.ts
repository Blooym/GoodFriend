import { NextFunction, Request, Response } from 'express';

export default (req: Request, res: Response, next: NextFunction) => {
  res.header('Deprecated', 'true');
  res.header('Warning', '299 - "Resource is deprecated"');
  next();
};
