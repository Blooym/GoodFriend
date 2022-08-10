import { Request, Response } from 'express';
import Client from '../../../Types/Client';

const userEventHandler = (req: Request, res: Response, clients: Client) => {
  const headers = {
    'Content-Type': 'text/event-stream',
    'Cache-Control': 'no-cache',
    Connection: 'keep-alive',
  };
  res.writeHead(200, headers);
  res.write('\n');

  clients.push({ ID: clients.length, res });

  res.on('close', () => {
    clients = clients.filter((client) => client.ID !== Date.now());
    res.end();
  });
};

export default userEventHandler;
