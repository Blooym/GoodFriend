<div align="center">

<img src="../../.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
# Good Friend Server Component

</div>

The GoodFriend server communicates with the plugin to allow clients to send login and logout events, and for other clients to recieve these events through the server. An instance is running by default and is already setup when the plugin is first installed, but if for whatever reason you need your own server, then read on.

## Prerequisites
- [Node.js](https://nodejs.org/en/)
- [Yarn](https://yarnpkg.com/getting-started/install)

Once you have these prerequisites installed, you can install the GoodFriend server dependencies by running `yarn`.

--- 
## Deployment: Localhost
To deploy the API on your localhost, you can run `yarn start`. This will start a server on port 3000 (by default). You can then set your plugin to use the local API by changing `Plugin Settings -> Show Advanced Settings -> API URL`. If you are not using SSL make sure you set it to use http not https.

---

## Deployment: Server
There are more steps involved to deploying on a production server, but the process should be pretty similar to the localhost deployment.

### Domain Name
You will have to assign a domain name to your server so that it can be assigned a certificate. This will be the domain name that is used in the plugin to connect to the API.

### Obtaining SSL Certificates
The easiest way to obtain a SSL certificate is to use Let's Encrypt. This will automatically generate a certificate for you, and you can use it to deploy your server. 

A good command line tool for this is [Certbot](https://certbot.eff.org/) which can generate and renew certificates for you. 

**Generate Certificate**
```
sudo certbot certonly --standalone
```

**Symlink/Copy Certificate**
```
sudo ln -s /etc/letsencrypt/live/<domain>/fullchain.pem ./SSL/fullchain.pem
sudo ln -s /etc/letsencrypt/live/<domain>/privkey.pem ./SSL/privkey.pem
```

**Set Certificate Location**
```
$EDITOR ./SSLConfig.ts
```

### Starting the Server
To start the server, set `NODE_ENV` to `production` or `SECURE=true` in your environment and then run `yarn start`.

Verify the server is running and using HTTPs by checking the domain associated with the API URL.

### Keeping the server running
To keep the server running in the background you can use a process manager like [PM2](https://pm2.io/) or [Forever](https://npmjs.com/package/forever).

---

## Available Environment Variables
- `PORT` - The port that the server will listen on. Defaults to `3000`.
- `SECURE` - Whether or not the server should use SSL. Defaults to `false`.
- `NODE_ENV` - The environment that the server will run in. Defaults to  `development`. (Production is mandatory SSL)
