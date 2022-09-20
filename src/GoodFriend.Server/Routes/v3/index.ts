import { Router } from 'express';

import Client from '@mtypes/Client';

import loginStateHandler from '@routes/v3/Handlers/PUT/loginstate';
import userEventHandler from '@routes/v3/Handlers/GET/events/users';
import clientsHandler from '@routes/v3/Handlers/GET/clients';

const router = Router();

const sseClients: Client = [];

router.put('/loginstate', (req, res) => loginStateHandler(req, res, sseClients));

router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
export { sseClients };
