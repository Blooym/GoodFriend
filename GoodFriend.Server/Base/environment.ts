import 'dotenv/config';

// API configuration options
export const PORT = process.env.PORT || 8000;
export const TRUST_PROXY = process.env.TRUST_PROXY || false;
export const RATELIMIT_RESET_MINS = process.env.RATELIMIT_RESET_MINS || 1;
export const REQUESTS_PER_RESET = process.env.REQUESTS_PER_RESET || 20;
export const AUTH_TOKEN = process.env.AUTH_TOKEN || '';

// Information provided through the v*/metadata endpoint.
export const { DONATION_PAGE_URL, STATUS_PAGE_URL, NEW_API_URL } = process.env;
