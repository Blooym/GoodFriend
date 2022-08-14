import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import isValidID from '@utils/Validators';

export default (req: Request, res: Response, clients: Client) => {
  if (!isValidID(req.params.id)) res.sendStatus(400);
  else {
    const event = {
      ContentID: req.params.id,
      LoggedIn: true,
    };

    res.sendStatus(200);
    clients.forEach((client) => { client.res.write(`data: ${JSON.stringify(event)}\n\n`); });
  }
};
