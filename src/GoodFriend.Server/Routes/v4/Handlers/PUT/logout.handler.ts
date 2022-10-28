import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import isValidID from '@utils/Validators';
import { totalSSEEventsSent, totalSSEStateEvents } from '@metrics/prometheus';

/**
 * Handles the logout event endpoint.
 * @param req The request object.
 * @param res The response object.
 * @param clients The list of clients.
 */
export default (req: Request, res: Response, clients: Client) => {
  const {
    contentID, homeworldID, territoryID, worldID, datacenterID,
  } = req.query;

  if (!isValidID(contentID as string) || !homeworldID || !territoryID) res.sendStatus(400);
  else {
    const event = {
      ContentID: contentID,
      HomeworldID: homeworldID,
      WorldID: worldID || null,
      DatacenterID: datacenterID || null,
      TerritoryID: territoryID,
      LoggedIn: false,
    };

    res.sendStatus(200);

    totalSSEStateEvents.inc({
      event: 'logout',
      homeworld: homeworldID as string,
      world: worldID as string,
      datacenter: datacenterID as string,
      territory: territoryID as string,
    });

    clients.forEach((client) => {
      client.res.write(`data: ${JSON.stringify(event)}\n\n`);
      totalSSEEventsSent.inc();
    });
  }
};
