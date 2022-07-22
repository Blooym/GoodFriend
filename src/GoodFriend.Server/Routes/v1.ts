// v1 routes
import { Router } from 'express';
import userEventHandler from '../Handlers/GET/events/users';
import loginHandler from '../Handlers/POST/login';
import logoutHandler from '../Handlers/POST/logout';
import Client from '../Types/Client';

const router = Router();
const clients: Client = [];

router.get('/events/users', (req, res) => userEventHandler(req, res, clients));
router.post('/login/:id', (req, res) => loginHandler(req, res, clients));
router.post('/logout/:id', (req, res) => logoutHandler(req, res, clients));

export default router;
