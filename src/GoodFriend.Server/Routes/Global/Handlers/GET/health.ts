import { Request, Response } from 'express';
import { freemem, totalmem, cpus } from 'os';

export default (req: Request, res: Response) => {
  const data = {
    processUptime: process.uptime(),
    memory: {
      usage: process.memoryUsage(),
      available: freemem(),
      total: totalmem() / 1024 / 1024,
    },
    cpu: {
      usage: process.cpuUsage().user / process.cpuUsage().system,
      threads: cpus().length,
      speed: cpus()[0].speed,
    },
  };

  res.send(data);
};
