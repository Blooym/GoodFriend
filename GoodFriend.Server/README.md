<div align="center">

<img src="../.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
# Good Friend API Component

</div>

The GoodFriend API communicates with the plugin and allows users to communicate with each other safely without leaking things like IP Addresses. Hosting your own instance of the server is easy and can be done in a few minutes containerized with Docker or manually with NodeJS.

Before anything, you must make sure that the Server & Plugin are compatible with each-other, otherwise you may run into issues with the plugin not being able to connect or send events to the API due to a mismatch in how the APIClient is trying to communicate. In most cases, the latest version of the plugin will work with the latest version of the API.

## Notes & Preamble
> **Note**  
> The API does not and will not support HTTPs natively, so you will always need to use a reverse proxy to handle that instead. You can use a tool like [Caddy](https://caddyserver.com/), [Traefik](https://traefik.io/), or [NGINX](https://www.nginx.com/) to handle this for you. Please note that no support will be provided for setting up a reverse proxy as there are many different ways to do this and it is out of the scope of this project.

You can run the API in a few different ways. The supported and easiest way is to use [Docker](https://www.docker.com/) as it will handle all of the dependencies & environment configuration for you. If you don't want to use Docker, you can also run the API manually with NodeJS (no support will be provided for this, but you can easily look at the [Dockerfile](./Dockerfile) to see how it's done).

### Running with Docker
> **Note**  
> This guide assumes you have Docker installed and you are at least somewhat familiar with how to use it. If you are not, please refer to the [Docker Documentation](https://docs.docker.com/) for more information.        

First off, you will want to make a new directory somewhere on your system to store the configuration files for the API. You can name this directory whatever you want, but for the sake of this guide we will call it `goodfriend`. 

Inside of this directory, you will want to create a new file called `.env` and copy the configuration from [`.env.example`](./.env.example) into it, filling in all the values as described in the comments. 

Once you have done that, you will want to create a new directory called `logs` inside of the `goodfriend` directory. This is where the API will store all of the runtime logs such as requests, errors, etc.

Now that you have the required directories and configuration files, you are ready to run the API. To do this, you will want to run the following command in the `goodfriend` directory:

```bash
docker run -d --name goodfriend-api --restart unless-stopped -v $(pwd)/logs:/app/logs --env-file .env ghcr.io/bitsofabyte/goodfriend:latest
```
You can forward the port of the container to your host machine by adding `-p PORT:HOST_PORT` to the command above. For example, if you wanted to forward port 8000 from the container to port 80 on your host machine, you would run `-p 8000:80`.

You can also substitute `latest` with a specific version of the API if you want to run a specific version. For example, if you wanted to run version `1.4.2.1`, you would run `ghcr.io/BitsOfAByte/goodfriend:1.4.2.1`.

### Accessing Logs & Metrics
The API exposes a few endpoints that can be used to access logs and metrics. These endpoints require the `AUTH_TOKEN` from the `.env` file to be passed in the `Authorization` header of the request as a `Bearer` token.

You can find the logs at `/logs` which will return a list of all the log files that are available. You can then access a specific log file by passing the filename in the `file` query parameter.

You can find the metrics at `/metrics` which will return a Prometheus formatted metrics file which can then be used to setup a dashboard with a tool like [Grafana](https://grafana.com/).
