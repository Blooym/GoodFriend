import { Request, Response } from 'express';

import isValidID from '@utils/Validators';
import ClientStore from '@data/ClientStore';

export default (req: Request, res: Response, clients: ClientStore) => {
  const ContentID = req.header('Player-ID') ?? String.fromCharCode(0);

  if (!isValidID(ContentID)) res.sendStatus(400);
  else {
    const event = {
      ContentID,
      LoggedIn: true,
    };

    res.sendStatus(200);
    clients.forEach((key: string, value: Response) => value.write(`data: ${JSON.stringify(event)}\n\n`));
  }
};
