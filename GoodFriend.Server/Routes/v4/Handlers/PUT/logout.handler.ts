import { Request, Response } from 'express';

import SSEClient from '@mtypes/SSEClient';
import { isValidContentIDHash, isValidIDUint } from '@utils/Validators';
import { totalSSEMesssagesSent, totalLoginstateUpdatesReceived } from '@services/Prometheus';
import UpdatePayload from '@mtypes/UpdatePayload';

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
    || !isValidIDUint(territoryID)
    || !isValidIDUint(worldID)
    || !isValidIDUint(datacenterID)) res.sendStatus(400);

  else {
    // Send a 200 OK response.
    res.sendStatus(200);

    // Prepare the response.
    const event: UpdatePayload = {
      ContentID: Number(contentID),
      WorldID: Number(worldID),
      DatacenterID: Number(datacenterID),
      TerritoryID: Number(territoryID),
      Salt: salt as string,
      LoggedIn: false,
    };

    // Increment some metrics.
    totalLoginstateUpdatesReceived.inc({
      type: 'logout',
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
