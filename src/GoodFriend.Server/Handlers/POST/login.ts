import { Request, Response } from 'express';
import Client from '../../Types/Client';
import isValidID from '../../Utils/Validators';

const loginHandler = (req: Request, res: Response, clients: Client) => {
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

export default loginHandler;
