import { Response } from 'express';

type SSEClient = {
    ID: string;
    res: Response
}[];

export default SSEClient;
