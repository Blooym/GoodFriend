<div align="center">

<img src="../../.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
# Good Friend API Component

</div>

The GoodFriend API communicates with the plugin and allows clients to communicate with eachother safely. It is designed to be as light-weight as possible and simple to run inside and outside of a container. Hosting your own instance of the server is easy and can be done in a few minutes.

Before anything, you must make sure that the server routes and plugin APIClient are compatiable with eachother, otherwise you may run into issues with the plugin not being able to connect or send events to the API due to a mismatch in how the APIClient is trying to communicate. You can normally just checkout most commit hashes or tags and the plugin and server will be compatiable, however this may not always be the case.


## Setup with Docker Container (Recommended)

Using a docker-container is the best way to run the server as it will automatically handle all of the dependencies and setup for you, and is typically how the server is run when testing and in production. 

### Prerequisites
- Docker
- Docker-Compose (Optional)

### Initial Setup
After cloning the repository locally, you will need to create an `.env` file in the `src/GoodFriend.Server` directory. This file will contain all of the environment variables that the server will use to run. You can use the `.env.example` file as a template for what variables you need to set.

### Running the Server
#### Docker-Compose
If you have docker-compose installed, you can run the server with the following command:

```bash
docker-compose up -d --build
```

This will build the docker image and run the container in the background for you, and will make sure the container is restarted if it crashes or the host machine restarts.

If you want update the server version, simply run the following command to get the latest code and rebuild the container:

```bash
git pull && docker-compose up -d --build
```

If you only want to update the environment variables without rebuilding the container, you can run the following command:

```bash
docker-compose up -d
```

#### Docker
If you do not have docker-compose installed, you will first need to build the server image with the following command:

```bash
docker build -t goodfriend-server .
```
    
Then you can run the container with the following command:    

```bash
docker run -d --name goodfriend-server --env-file .env goodfriend-server
```

#### Uncontainerized

> **Warning**
> This method is not recommended or supported.

If you do not want to use docker, you can run the server directly on your host machine. You will need to make sure that you have the following dependencies installed:

- [Node.js](https://nodejs.org/)    
- [Yarn](https://yarnpkg.com/)

You will then need to install the dev-dependencies using `yarn install` and then build the project using `yarn build`. From here you can run the server using `yarn start`, or you can remove the `node_modules` folder and install the production dependencies using `yarn install --production` and then run the server using `node ./dist/index.js`.

### Logs & Metrics
The server will automatically create a `logs` directory wherever the `process.cwd()` is when the server is started. This directory will contain all of the logs for errors and events, logs are also output *stdout* and *stderr*.

You can view the logs for the server from anywhere by sending an authenticated request to the `/logs` endpoint, which will allow you to view or download any log files that are available.

The server also exposes a `/metrics` endpoint which will return prometheus compatible metrics for the server. THis can be used to setup any dashboard or monitoring system you want to use. Please keep in mind that the metrics endpoint requires authentication.
