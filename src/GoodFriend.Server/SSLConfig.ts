// If you are running on localhost, this does not need to be set.
// If you are running on a remote server, either set the environment variable or
// Set the path manually after the || line below.

export const certFile: string = process.env.CERTFILE || '';
export const keyFile: string = process.env.KEYFILE || '';
