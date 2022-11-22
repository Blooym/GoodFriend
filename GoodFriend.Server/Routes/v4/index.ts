import { Router } from 'express';

import SSEClient from '@mtypes/SSEClient';

import loginHandler from '@routes/v4/Handlers/PUT/login.handler';
import logoutHandler from '@routes/v4/Handlers/PUT/logout.handler';
import sseFriendEventHandler from '@routes/v4/Handlers/GET/SSE/friends.handler';
import metadata from '@routes/v4/Handlers/GET/metadata.handler';

const router: Router = Router();
const sseClients: SSEClient = [];

router.put('/login', (req, res) => loginHandler(req, res, sseClients));
router.put('/logout', (req, res) => logoutHandler(req, res, sseClients));

router.get('/metadata', (req, res) => metadata(req, res, sseClients));
router.get('/sse/friends', (req, res) => sseFriendEventHandler(req, res, sseClients));

export default router;
