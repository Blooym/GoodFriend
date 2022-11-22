import { Request, Response } from 'express';

import SSEClient from '@mtypes/SSEClient';
import {
  MAX_SSE_CONNECTIONS, METADATA_DONATION_PAGE_URL, METADATA_NEW_API_URL, METADATA_STATUS_PAGE_URL,
} from '@common/environment';

/**
 * Handles the v4/metadata endpoint.
 * @param req The request object.
 * @param res The response object.
 * @param sseClients The list of clients.
 */
export default async (req: Request, res: Response, sseClients: SSEClient) => {
  const response = {
    connectedClients: sseClients.length,
    maxCapacity: MAX_SSE_CONNECTIONS,
    donationPageUrl: METADATA_DONATION_PAGE_URL,
    statusPageUrl: METADATA_STATUS_PAGE_URL,
    newApiUrl: METADATA_NEW_API_URL,
  };

  res.status(200).json(response);
};
