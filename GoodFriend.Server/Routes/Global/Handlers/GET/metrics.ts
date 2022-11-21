import { Request, Response } from 'express';
import register from 'Metrics/prometheus';

/**
 * Serves the metrics for Prometheus.
 * @param req The request object.
 * @param res The response object.
 */
export default async (req: Request, res: Response) => {
  res.setHeader('Content-Type', register.contentType);
  res.end(await register.metrics());
};
