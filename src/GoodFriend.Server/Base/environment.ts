import 'dotenv/config';

export const PORT = process.env.PORT || 8000;
export const TRUST_PROXY = process.env.TRUST_PROXY || false;
export const RATELIMIT_RESET_MINS = process.env.RATELIMIT_RESET_MINS || 1;
export const REQUESTS_PER_RESET = process.env.REQUESTS_PER_RESET || 20;
export const LOG_DAYS_TO_KEEP = process.env.LOG_DAYS_TO_KEEP || 7;
export const AUTH_TOKEN = process.env.AUTH_TOKEN || '';
export const BANNED_USERAGENTS = process.env.BANNED_USERAGENTS?.split(',') || [];
