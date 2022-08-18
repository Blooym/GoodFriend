import { Router } from 'express';

import logoutHandler from '@routes/v2/Handlers/PUT/logout';
import userEventHandler from '@routes/v2/Handlers/GET/events/users';
import loginHandler from '@routes/v2/Handlers/PUT/login';
import clientsHandler from '@routes/v2/Handlers/GET/clients';
import ClientStore from '@data/ClientStore';
import UseDriver from '@utils/UseDriver';

require('dotenv').config();

const router = Router();

// wait for the async function to complete before continuing
const data = new ClientStore(UseDriver());

router.put('/login', (req, res) => loginHandler(req, res, data));
router.put('/logout', (req, res) => logoutHandler(req, res, data));

router.get('/clients', (req, res) => clientsHandler(req, res, data));
router.get('/events/users', (req, res) => userEventHandler(req, res, data));

export default router;
