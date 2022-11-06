import { Response } from 'express';

type Client = {
    ID: string;
    res: Response
}[];

export default Client;
