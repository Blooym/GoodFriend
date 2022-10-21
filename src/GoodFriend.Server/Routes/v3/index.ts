import { Router } from 'express';

import Client from '@mtypes/Client';

import loginHandler from '@routes/v3/Handlers/PUT/login.handler';
import logoutHandler from '@routes/v3/Handlers/PUT/logout.handler';
import clientsHandler from '@routes/v3/Handlers/GET/clients.handler';
import userEventHandler from '@routes/v3//Handlers/GET/SSE/events.handler';

const router = Router();

const sseClients: Client = [];

router.put('/login', (req, res) => loginHandler(req, res, sseClients));
router.put('/logout', (req, res) => logoutHandler(req, res, sseClients));

router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
