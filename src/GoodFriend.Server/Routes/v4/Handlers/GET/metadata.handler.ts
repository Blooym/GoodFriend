import { Request, Response } from 'express';

import Client from '@mtypes/Client';
import { DONATION_PAGE_URL, NEW_API_URL, STATUS_PAGE_URL } from '@base/environment';

/**
 * Handles the v4/metadata endpoint.
 * @param req The request object.
 * @param res The response object.
 * @param clients The list of clients.
 */
export default async (req: Request, res: Response, clients: Client) => {
  const response = {
    connectedClients: clients.length,
    donationPageUrl: DONATION_PAGE_URL || null,
    statusPageUrl: STATUS_PAGE_URL || null,
    newApiUrl: NEW_API_URL || null,
  };

  res.status(200).json(response);
};
