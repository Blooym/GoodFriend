import { Response } from 'express';

type Client = {
    ID: number,
    res: Response
}[];

export default Client;
