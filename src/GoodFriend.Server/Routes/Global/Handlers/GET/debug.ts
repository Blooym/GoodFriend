import { Request, Response } from 'express';
import os from 'os';

export default (req: Request, res: Response) => {
  const cpuUsage = (process.cpuUsage().user / process.cpuUsage().system).toFixed(2);
  const memUsage = (process.memoryUsage().rss / 1024 / 1024).toFixed(2);
  const totalMem = (os.totalmem() / 1024 / 1024).toFixed(2);

  const data = {
    memUsed: memUsage,
    memTotal: totalMem,
    cpuUsage,
    processUptime: process.uptime(),
  };

  res.send(data);
};
