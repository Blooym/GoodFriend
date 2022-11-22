import 'dotenv/config';
import { GenerateRandomString } from '@utils/Common';

// Basic App Configuration
export const PORT = process.env.PORT || 8000;
export const NODE_ENV = process.env.NODE_ENV || 'development';
export const MAX_SSE_CONNECTIONS = parseInt(process.env.MAX_SSE_CONNECTIONS || '0', 10);

// Security Configuration
export const TRUST_PROXY = process.env.SECURITY_TRUST_PROXY || false;
export const SECURITY_AUTH_TOKEN = process.env.SECURITY_AUTH_TOKEN || GenerateRandomString(128);
export const SECURITY_AUTH_EVERYWHERE = process.env.SECURITY_AUTH_EVERYWHERE || false;

// Ratelimit Configuration
export const RATELIMIT_REQUESTS_PER_RESET = process.env.RATELIMIT_REQUESTS_PER_RESET || 80;
export const RATELIMIT_RESET_MINS = process.env.RATELIMIT_RESET_MINS || 60;
export const RATELIMIT_IGNORE_AUTHENTICATED = process.env.RATELIMIT_IGNORE_AUTHENTICATED || true;

// Logging Configuration
export const LOGGING_KEEP_FOR_DAYS = process.env.LOGGING_KEEP_FOR_DAYS || 7;
export const LOG_PATH = './logs';

// Metadata Configuration
export const {
  METADATA_DONATION_PAGE_URL,
  METADATA_STATUS_PAGE_URL,
  METADATA_NEW_API_URL,
} = process.env;
