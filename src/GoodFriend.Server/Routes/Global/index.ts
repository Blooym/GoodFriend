import { Router } from 'express';

import AuthenticatedEndpoint from '@middleware/Authenticated';

import health from '@routes/Global/Handlers/GET/health';
import logs from '@routes/Global/Handlers/GET/logs';

const router = Router();

router.get('/', (req, res) => res.sendStatus(200));
router.get('/health', AuthenticatedEndpoint, health);
router.get('/logs', AuthenticatedEndpoint, logs);

export default router;
