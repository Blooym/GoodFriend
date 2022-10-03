import { Router } from 'express';

import AuthenticatedEndpoint from '@middleware/Authenticated';

import logs from '@routes/Global/Handlers/GET/logs';
import metrics from '@routes/Global/Handlers/GET/metrics';

const router = Router();

router.get('/', (req, res) => res.sendStatus(200));
router.get('/metrics', AuthenticatedEndpoint, metrics);
router.get('/logs', AuthenticatedEndpoint, logs);

export default router;
