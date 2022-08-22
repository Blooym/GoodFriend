import { Request, Response } from 'express';

import fs from 'fs';
import path from 'path';

const LOG_PATH = process.env.LOG_PATH || 'logs';

/**
 * Returns all log files inside of the log directory.
 * @param req The request object to get the parameters from.
 * @param res The response object to send the response to.
 */
const showAllFiles = (req: Request, res: Response) => {
  // Make the dir if it doesnt exist already, this shouldn't usually happen.
  if (!fs.existsSync(LOG_PATH)) {
    fs.mkdirSync(LOG_PATH);
  }

  // Read the directory and return all files.
  fs.readdir(LOG_PATH, (err, files) => {
    if (err) {
      res.status(500).send(err);
    } else {
      const url = `${req.protocol}://${req.get('host')}${req.originalUrl}`;
      const resp = files.map((file) => ({
        name: file,
        size: fs.statSync(`${LOG_PATH}/${file}`).size,
        view: {
          download: `${url}?file=${file}&type=download`,
          plain: `${url}?file=${file}&type=plain`,
          json: `${url}?file=${file}&type=json`,
        },
      })).filter((file) => file.name.endsWith('.log'));

      // Send back the response if its not null, otherwise send a message.
      if (resp.length > 0) {
        res.json(resp);
      } else res.status(404).send('No log were detected.');
    }
  });
};

/**
 * Returns the contents of the log file according to the given request parameters.
 * @param req The request object to get the parameters from.
 * @param res The response object to send the response to.
 * @param logFile The full log file path to access.
 */
const showLogFile = (req: Request, res: Response, logFile: string) => {
  const returnType = req.query.type;
  const sortType = req.query.sort || 'asc';
  const contentLimit = Number(req.query.limit) || Number.MAX_SAFE_INTEGER;
  const contentFilter = req.query.filter || '';

  // Read the file and format its contents according to the request parameters.
  const file = fs.readFileSync(logFile);
  let content = file.toString().split('\n').slice(0, contentLimit).join('\n');
  content = sortType === 'desc' ? content.split('\n').reverse().join('\n') : content;
  content = contentFilter
    ? content
      .split('\n')
      .filter((line) => line.includes(contentFilter.toString()))
      .join('\n')
    : content;
  content = content.trimEnd();

  // Return it as the requested type or fallback to plain text.
  switch (returnType) {
    case 'json':
      res.json({
        file: req.query.file,
        size: file.length,
        contents: content.split('\n').map((line) => line),
      });
      break;
    case 'download':
      res.setHeader(
        'Content-disposition',
        `attachment; filename=${req.query.file}`,
      );
      res.setHeader('Content-type', 'text/plain');
      res.send(content);
      break;
    default:
      res.setHeader('Content-Type', 'text/plain');
      res.send(content);
      break;
  }
};

/**
 * Returns if the requested log file is available & valid to be accessed.
 * @param res The response object to send the response to.
 * @param logFile The full log file path to validate.
 */
const validLogFile = (res: Response, logFile: string) => path.parse(logFile).dir.endsWith(LOG_PATH) && logFile.endsWith('.log');

/**
 * Handles the GET request to the /logs endpoint.
 */
export default (req: Request, res: Response) => {
  const reqFile = req.query.file;

  // No specific file requested, show all files
  if (!reqFile) {
    showAllFiles(req, res);
    return;
  }

  // If the file is valid, send it, otherwise return a 404.
  const logFile = `${LOG_PATH}/${req.query.file}`;
  if (validLogFile(res, logFile)) {
    if (fs.existsSync(logFile)) showLogFile(req, res, logFile);
    else res.status(404).send(`File ${reqFile} not found.`);
  } else res.sendStatus(404);
};
