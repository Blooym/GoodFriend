import { Request, Response } from 'express';

import fs from 'fs';
import path from 'path';

const LOGDIR = `${process.cwd()}/logs`;

/**
 * Returns all log files inside of the log directory.
 * @param req The request object to get the parameters from.
 * @param res The response object to send the response to.
 */
const showAllFiles = (req: Request, res: Response) => {
  fs.readdir(LOGDIR, (err, files) => {
    if (err) {
      res.status(500).send(err);
    } else {
      const url = `${req.protocol}://${req.get('host')}${req.originalUrl}`;

      res.send(files.map((file) => ({
        name: file,
        size: fs.statSync(`${LOGDIR}/${file}`).size,
        download: `${url}?file=${file}&type=download`,
        view: `${url}?file=${file}&type=plain`,
        json: `${url}?file=${file}&type=json`,
      })));
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
  content = contentFilter ? content.split('\n').filter((line) => line.includes(contentFilter.toString())).join('\n') : content;
  content = content.trimEnd();

  // Return it as the requested type or fallback to plain text.
  switch (returnType)
  {
    case 'json':
      res.json({
        file: req.query.file,
        size: file.length,
        contents: content.split('\n').map((line) => (line)),
      });
      break;
    case 'download':
      res.setHeader('Content-disposition', `attachment; filename=${req.query.file}`);
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
 * Validates the log file path to make sure it doesn't escape the logs directory
 * or try and invalid files.
 * @param res The response object to send the response to.
 * @param logFile The full log file path to validate.
 */
const validLogFile = (res: Response, logFile: string) => {
  if (!path.parse(logFile).dir.endsWith(LOGDIR) || !logFile.endsWith('.log')) {
    res.sendStatus(403);
  }
};

/**
 * Handles the GET request to the /logs endpoint.
 */
export default (req: Request, res: Response) => {
  const reqFile = req.query.file;

  // No specific file requested, show all files
  if (!reqFile) { showAllFiles(req, res); return; }

  // Specific file requested, fetch and validate first
  const logFile = `${LOGDIR}/${req.query.file}`;
  validLogFile(res, logFile);

  // File is valid, check if it exists and show it
  if (fs.existsSync(logFile)) showLogFile(req, res, logFile);
  else res.status(404).send(`File ${reqFile} not found.`);
};
