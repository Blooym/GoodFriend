import { Request, Response } from 'express';
import { v4 } from 'uuid';

import ClientStore from '@data/ClientStore';

const HEARTBEAT_INTERVAL = 2000;
const HEADERS = {
  'Content-Type': 'text/event-stream',
  'Cache-Control': 'no-cache',
  Connection: 'keep-alive',
  'X-Accel-Buffering': 'no',
};

export default (req: Request, res: Response, clients: ClientStore) => {
  const UUID = v4();
  const interval = setInterval(() => {
    res.write(':\n\n');
  }, HEARTBEAT_INTERVAL);

  // Send the client a response and add them to the list of clients.
  res.writeHead(200, HEADERS);
  res.write(':\n\n');
  clients.addKey(UUID, res);

  res.on('close', () => {
    res.end();
    clients.delKey(UUID);
    clearInterval(interval);
  });
};
