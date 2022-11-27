import { METADATA_DONATION_PAGE_URL, METADATA_NEW_API_URL, METADATA_STATUS_PAGE_URL } from '@common/environment';
import { Request, Response } from 'express';

/**
 * Serves an index page for the / route.
 * @param req The request object.
 * @param res The response object.
 */
export default async (req: Request, res: Response) => {
  res.status(200).send(`
    <!DOCTYPE html>
      <head>
        <title>GoodFriend API </title>
      </head>
      <body>
        ${METADATA_NEW_API_URL && METADATA_NEW_API_URL !== `${req.protocol}://${req.get('host')}` ? `
          <h1>This API is has been moved to a new location!</h1>
          <p>It is recommended that you update your client to use the new API location at <a href="${METADATA_NEW_API_URL}">${METADATA_NEW_API_URL}</a> instead of this one.</p>
          <hr />
        ` : ''}

        <div class="heading">
          <h1>GoodFriend</h1>
          <p>GoodFriend is an API Event relay for communicating friend login and logout events between multiple clients for use in-game.</p>
        </div>

        <div class="using-the-api">
          <h2>Using this API Instance</h2>
          <p>If you would like to use this API instance, set your client to use the following URL: <code>${METADATA_NEW_API_URL || `${req.protocol}://${req.get('host')}`}</code></p>
        </div>

        <div class="more-info">
          ${METADATA_STATUS_PAGE_URL || METADATA_DONATION_PAGE_URL ? '<h2>More Information</h2>' : ''}
          ${METADATA_STATUS_PAGE_URL ? `<p>The API instance status page is available at <a href="${METADATA_STATUS_PAGE_URL}">${METADATA_STATUS_PAGE_URL}</a>.</p>` : ''}
          ${METADATA_DONATION_PAGE_URL ? `<p>The API instance donation page is available at <a href="${METADATA_DONATION_PAGE_URL}">${METADATA_DONATION_PAGE_URL}</a>.</p>` : ''}
        </div>

        <div class="footer">
          <br />
          <hr />
          <p>GoodFriend is a project by <a href="https://github.com/BitsOfAByte">BitsOfAByte</a> and is licensed under the <a href="https://github.com/BitsOfAByte/GoodFriend/blob/main/LICENSE">AGPL3.0 License</a>. Source code is available on <a href="https://github.com/BitsOfAByte/GoodFriend">GitHub</a>.</p>
        </div>
      </body>
    </html>
  `);
};
