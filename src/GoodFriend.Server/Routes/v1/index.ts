import { Router } from 'express';

import Client from '@mtypes/Client';

import Deprecated from '@middleware/Deprecated';

import logoutHandler from '@routes/v1/Handlers/POST/logout';
import userEventHandler from '@routes/v1/Handlers/GET/events/users';
import loginHandler from '@routes/v1/Handlers/POST/login';
import clientsHandler from '@routes/v1/Handlers/GET/clients';

const router = Router();

const sseClients: Client = [];

router.use(Deprecated); // Mark the route as deprecated so clients can handle it.

router.post('/login/:id', (req, res) => loginHandler(req, res, sseClients));
router.post('/logout/:id', (req, res) => logoutHandler(req, res, sseClients));

router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
