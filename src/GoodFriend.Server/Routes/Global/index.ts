import { Router } from 'express';

import AuthenticatedEndpoint from '@middleware/Authenticated';

import debug from '@routes/Global/Handlers/GET/debug';
import logs from '@routes/Global/Handlers/GET/logs';

const router = Router();

router.get('/', (req, res) => res.sendStatus(200));
router.get('/debug', AuthenticatedEndpoint, debug);
router.get('/logs', AuthenticatedEndpoint, logs);

export default router;
