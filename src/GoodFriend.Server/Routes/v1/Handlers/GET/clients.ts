import { Request, Response } from 'express';

import Client from '@mtypes/Client';

const HEADERS = {
  'Cache-Control': 'must-revalidate,max-age=60',
};

export default (req: Request, res: Response, clients: Client) => {
  res.header(HEADERS).send({ clients: clients.length });
};
