import { Router } from 'express';

import AuthenticatedEndpoint from '@middleware/Authenticated';

import debug from '@routes/Global/Handlers/GET/debug';

const router = Router();

router.get('/', (req, res) => res.sendStatus(200));
router.get('/debug', AuthenticatedEndpoint, debug);

export default router;
