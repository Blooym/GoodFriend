import { Request, Response } from 'express';

import ClientStore from '@data/ClientStore';

const HEADERS = {
  'Cache-Control': 'must-revalidate',
};

export default (req: Request, res: Response, clients: ClientStore) => {
  clients.length().then((length: number) => {
    res.header(HEADERS).send({ clients: length });
  });
};
