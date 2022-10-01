import { Router } from 'express';

import Client from '@mtypes/Client';

import loginHandler from '@routes/v3/Handlers/PUT/login';
import logoutHandler from '@routes/v3/Handlers/PUT/logout';
import clientsHandler from '@routes/v3/Handlers/GET/clients';
import userEventHandler from '@routes/v3//Handlers/GET/events/users';

const router = Router();

const sseClients: Client = [];

router.put('/login', (req, res) => loginHandler(req, res, sseClients));
router.put('/logout', (req, res) => logoutHandler(req, res, sseClients));

router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
