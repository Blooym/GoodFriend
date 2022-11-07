import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import { isValidContentIDHash, isValidIDUint } from '@utils/Validators';
import { totalSSEEventsSent, totalSSEStateEvents } from '@metrics/prometheus';

/**
 * Handles the login event endpoint.
 * @param req The request object.
 * @param res The response object.
 * @param clients The list of clients.
 */
export default (req: Request, res: Response, clients: Client) => {
  const {
    contentID, homeworldID, territoryID, worldID, datacenterID,
  } = req.query;

  if (!isValidContentIDHash(contentID as string)
  || !isValidIDUint(homeworldID as string)
  || !isValidIDUint(territoryID as string)
  || !isValidIDUint(worldID as string)
  || !isValidIDUint(datacenterID as string)) res.sendStatus(400);

  else {
    const event = {
      ContentID: contentID,
      HomeworldID: homeworldID,
      WorldID: worldID,
      DatacenterID: datacenterID,
      TerritoryID: territoryID,
      LoggedIn: true,
    };

    res.sendStatus(200);

    totalSSEStateEvents.inc({
      event: 'login',
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
