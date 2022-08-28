import { Router } from 'express';

import Client from '@mtypes/Client';

import logoutHandler from '@routes/v2/Handlers/PUT/logout';
import userEventHandler from '@routes/v2/Handlers/GET/events/users';
import loginHandler from '@routes/v2/Handlers/PUT/login';
import clientsHandler from '@routes/v2/Handlers/GET/clients';

const router = Router();

const sseClients: Client = [];

router.put('/login', (req, res) => loginHandler(req, res, sseClients));
router.put('/logout', (req, res) => logoutHandler(req, res, sseClients));

router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
export { sseClients };
