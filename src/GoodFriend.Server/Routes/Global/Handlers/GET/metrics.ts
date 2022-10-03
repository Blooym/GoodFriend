import { Request, Response } from 'express';
import register from 'Metrics/prometheus';

export default async (req: Request, res: Response) => {
  res.setHeader('Content-Type', register.contentType);
  res.end(await register.metrics());
};
