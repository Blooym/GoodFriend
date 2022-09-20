import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import isValidID from '@utils/Validators';
import LoginStates from '@mtypes/LoginState';

export default (req: Request, res: Response, clients: Client) => {
  const { state, contentID } = req.query;

  if (!state || !contentID) {
    res.status(400).send('Invalid request, missing parameters');
    return;
  }

  if (!isValidID(contentID as string) || !Object.values(LoginStates).includes(state as string)) {
    res.sendStatus(400);
  }
  else {
    const event = {
      ContentID: contentID,
      LoginState: state,
    };

    res.sendStatus(200);
    clients.forEach((client) => { client.res.write(`data: ${JSON.stringify(event)}\n\n`); });
  }
};
