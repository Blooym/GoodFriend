import { Router } from 'express';

import Client from '@mtypes/Client';

import logoutHandler from '@routes/v1/Handlers/POST/logout';
import userEventHandler from '@routes/v1/Handlers/GET/events/users';
import loginHandler from '@routes/v1/Handlers/POST/login';
import clientsHandler from '@routes/v1/Handlers/GET/clients';

const router = Router();

// SSE Clients are per-router and are used to send events to.
const sseClients: Client = [];

// POST
router.post('/login/:id', (req, res) => loginHandler(req, res, sseClients));
router.post('/logout/:id', (req, res) => logoutHandler(req, res, sseClients));

// GET
router.get('/clients', (req, res) => clientsHandler(req, res, sseClients));
router.get('/events/users', (req, res) => userEventHandler(req, res, sseClients));

export default router;
