import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import isValidID from '@utils/Validators';
import { totalSSEEventsSent, totalSSEStateEvents } from '@metrics/prometheus';

export default (req: Request, res: Response, clients: Client) => {
  const { contentID, homeworldID, territoryID } = req.query;

  if (!isValidID(contentID as string) || !homeworldID || !territoryID) res.sendStatus(400);
  else {
    const event = {
      ContentID: contentID,
      HomeworldID: homeworldID,
      TerritoryID: territoryID,
      LoggedIn: true,
    };

    res.sendStatus(200);

    totalSSEStateEvents.inc({
      event: 'login',
      homeworld: homeworldID as string,
      territory: territoryID as string,
    });

    clients.forEach((client) => {
      client.res.write(`data: ${JSON.stringify(event)}\n\n`);
      totalSSEEventsSent.inc();
    });
  }
};
