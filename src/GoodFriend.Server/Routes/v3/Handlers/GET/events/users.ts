import { Request, Response } from 'express';
import { v4 } from 'uuid';

import { totalSSEClients, averageSSESessionLength, totalSSESessionTime } from '@metrics/prometheus';
import Client from '@mtypes/Client';

const HEARTBEAT_INTERVAL = 30000;
const HEADERS = {
  'Content-Type': 'text/event-stream',
  'Cache-Control': 'no-cache',
  Connection: 'keep-alive',
  'X-Accel-Buffering': 'no',
};

export default (req: Request, res: Response, clients: Client) => {
  // Setup a heartbeat to keep the connection alive.
  const interval = setInterval(() => {
    res.write(':\n\n');
  }, HEARTBEAT_INTERVAL);
  const UUID = v4();
  const stopPlaytimeTimer = averageSSESessionLength.startTimer();

  // Send the client a response and add them to the list of clients.
  res.writeHead(200, HEADERS);
  res.write(':\n\n');
  clients.push({ ID: UUID, res });
  totalSSEClients.inc();

  // When the connection is closed remove, dispose of resources & remove the client.
  res.on('close', () => {
    res.end();
    clients.splice(clients.findIndex((client) => client.ID === UUID), 1);
    totalSSEClients.dec();
    averageSSESessionLength.observe(stopPlaytimeTimer());
    totalSSESessionTime.inc(stopPlaytimeTimer());
    clearInterval(interval);
  });
};
