import { Router } from 'express';

import AuthenticatedEndpoint from '@middleware/Authenticated';

import root from '@routes/Global/Handlers/GET/root';
import logs from '@routes/Global/Handlers/GET/logs';
import metrics from '@routes/Global/Handlers/GET/metrics';

const router = Router();

router.get('/', root);
router.get('/metrics', AuthenticatedEndpoint, metrics);
router.get('/logs', AuthenticatedEndpoint, logs);

export default router;
