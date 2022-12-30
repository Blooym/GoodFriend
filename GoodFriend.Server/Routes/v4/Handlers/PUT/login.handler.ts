import { Request, Response } from 'express';

import SSEClient from '@mtypes/SSEClient';
import UpdatePayload from '@mtypes/UpdatePayload';
import { isValidContentIDHash, isValidIDUint } from '@utils/Validators';
import { totalSSEMesssagesSent, totalLoginstateUpdatesReceived } from '@services/Prometheus';

/**
 * Handles the login event endpoint.
 * @param req The request object.
 * @param res The response object.
 * @param sseClients The list of clients.
 */
export default (req: Request, res: Response, sseClients: SSEClient) => {
  const {
    contentID, territoryID, worldID, datacenterID, salt,
  } = req.query;

  // If the response is not valid, return a 400 Bad Request.
  if (!isValidContentIDHash(contentID as string)
    || !isValidIDUint(territoryID as string)
    || !isValidIDUint(worldID as string)
    || !isValidIDUint(datacenterID as string)) res.sendStatus(400);

  else {
    // Send a 200 OK response.
    res.sendStatus(200);

    const event: UpdatePayload = {
      ContentID: Number(contentID),
      WorldID: Number(worldID),
      DatacenterID: Number(datacenterID),
      TerritoryID: Number(territoryID),
      Salt: salt as string,
      LoggedIn: true,
    };

    // Increment some metrics.
    totalLoginstateUpdatesReceived.inc({
      type: 'login',
      world: worldID as string,
      datacenter: datacenterID as string,
      territory: territoryID as string,
    });

    // Send the event to all clients.
    sseClients.forEach((client) => {
      client.res.write(`data: ${JSON.stringify(event)}\n\n`);
      totalSSEMesssagesSent.inc();
    });
  }
};
