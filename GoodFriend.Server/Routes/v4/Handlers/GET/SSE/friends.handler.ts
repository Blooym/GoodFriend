import { Request, Response } from 'express';
import { v4 } from 'uuid';

import SSEClient from '@mtypes/SSEClient';
import { SSE_HEADERS } from '@common/headers';
import { allTimeSSEClients, currentSSEClients, totalSSESessionTime } from '@services/Prometheus';
import { MAX_SSE_CONNECTIONS } from '@common/environment';

const HEARTBEAT_INTERVAL = 30000;

/**
 * Handles & Serves the SSE stream for friend events.
 * @param req The request object.
 * @param res The response object.
 * @param sseClients The list of clients.
 */
export default (req: Request, res: Response, sseClients: SSEClient) => {
  // If the server is at max capacity, return a 503 Service Unavailable.
  if (sseClients.length >= MAX_SSE_CONNECTIONS && MAX_SSE_CONNECTIONS !== 0) {
    res.status(503).send('The server is currently at maximum capacity for connections. Please try again later.');
    return;
  }

  // Generate a UUID for this client, send a 200 OK response, and set the headers.
  const UUID = v4();
  const time = Date.now();
  res.writeHead(200, SSE_HEADERS);

  // Write a heartbeat and add the client to the ssClients list.
  res.write(':\n\n');
  sseClients.push({ ID: UUID, res });

  // Increment some metrics.
  currentSSEClients.inc();
  allTimeSSEClients.inc();

  // Set up a heartbeat interval to keep the connection alive or close it if the client disconnects.
  const interval = setInterval(() => { res.write(':\n\n'); }, HEARTBEAT_INTERVAL);

  // Begin listening for the client to close the connection.
  res.on('close', () => {
    // Clear the heartbeat interval.
    clearInterval(interval);

    // End the connection and remove the client from the list.
    res.end();
    sseClients.splice(sseClients.findIndex((client) => client.ID === UUID), 1);

    // Update some metrics.
    currentSSEClients.dec();
    totalSSESessionTime.inc(Date.now() - time);
  });
};
