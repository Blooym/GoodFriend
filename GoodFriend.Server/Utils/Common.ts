import { SECURITY_AUTH_TOKEN } from '@common/environment';
import { randomBytes } from 'crypto';

const IsAuthenticated = (token: string | null | undefined) => token?.replace('Bearer ', '') === SECURITY_AUTH_TOKEN;

export const GenerateRandomString = (length: number) => randomBytes(length).toString('base64');

export default IsAuthenticated;
