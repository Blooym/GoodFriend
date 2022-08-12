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
To deploy the API on your localhost, you can run `yarn start`. This will start a server on port 8000 (by default). You can then set your plugin to use the local API by changing `Plugin Settings -> Show Advanced Settings -> API URL`. If you are not using SSL make sure you set it to use http not https.

---

## Deployment: Server
There are more steps involved to deploying on a production server, but the process shouldn't be too complicated.

### Environment Variables / Config
Change the file named `.env.example` to `.env` and fill in its values with the appropriate values for your server, please keep in mind that the `KEYFILE` and `CERTFILE` must be accessible by the node process when it is running, if you choose to use docker they must also be accessible by the docker container.

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
$EDITOR ./.env
```

### Running (Docker)
If you want to build and run the server inside of a container, first check the `Dockerfile` and adjust the port, then run the following to build the docker image.
```
docker build -t goodfriend-api .
```
Afterwards, run the following to run the container.
```
docker run -d goodfriend-api
```
*Note: if you are running on a Linux machine, you may need to add `--network="host"` to the command to forward the port.*
Environment variables set in the `Dockerfile` will override the ones set in the `.env` file.

### Running (Non-Docker)
Set the values inside of the `.env` file and then run `yarn start` to start the server.