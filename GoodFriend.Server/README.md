<div align="center">

<img src="../.assets/icon.png" alt="GoodFriend Logo" width="15%">
  
# GoodFriend: Server/API

</div>

The GoodFriend API (or "server") provides a way for clients of GoodFriend-compatiable plugins (such as [GoodFriend.Plugin](../GoodFriend.Plugin/) from this repository) to communicate with each-other without the need for peer-to-peer communication. This is achieved by sending events that occur on one clients machine to other clients. This works around some of the in-game limitations regarding friend data; essentially acting as a "relay" server between clients to protect individual privacy. Due to the nature of this design, most of the functionality provided by the server can only be utilised if both sides (e.g. another friend) are using a GoodFriend-compatiable plugin.

The API is easily self-hostable with little work needed to get it going (to make sure that even if the official instance goes away others can host their own). The API could also be entirely rewritten in another language and still work with any GoodFriend-compatiable plugin, as long as it inputs/outputs the same data.

## Features

- Ratelimit support out-of-the-box.
- Lightweight and easy on system resources
- Easy to selfhost with Docker (or other containerisation software that support Dockerfile).
- Proxy support (for use behind a reverse proxy).
- Prometheus metrics setup out-of-the-box (available at `/metrics`).
- Logging support using `winston-express` (files available at `/logs`).
- Support for setting a maximum number of clients to the server connected at once (for low-resource systems).

## Selfhosting

### Notes & Preamble

> **Note**  
> The API does not and will not support HTTPs natively. You will need to use a reverse proxy to handle that instead such as [Caddy](https://caddyserver.com/), [Traefik](https://traefik.io/), or [NGINX](https://www.nginx.com/). Please note that no support will be provided for setting up a reverse proxy as there are many ways to do this and it is out of the scope of this project.

You can run the API in a few ways. The supported and easiest way is to use [Docker](https://www.docker.com/) as it will handle all of the dependencies & environment configuration for you. If you don't want to use Docker, you can also run the API manually with NodeJS (no support will be provided for this, but you can easily look at the [Dockerfile](./Dockerfile) to see how it's done).

### Running with Docker

> **Note**  
> This guide assumes you have Docker (or other docker-compliant tool) installed and you are at least somewhat familiar with how to use it. Please refer to the [Docker Documentation](https://docs.docker.com/) if you need help with following this guide.

First off, you will want to make a new directory somewhere on your system to store the configuration and log files for the API. This can be anywhere on your system, but for the sake of this guide we will assume you are using `~/goodfriend` as the directory.

Inside of this directory, you will want to create a new file called `.env` and copy the configuration from [`.env.example`](./.env.example) into it. All of the configuration options are documented inside of the example file.

After this, you will want to make a new directory called `logs`, which will be used to store the log files for the API persistently across restarts. If you don't want to keep logs across restarts then feel free to skip this step.

Once you've completed these steps, you can start the latest version of the API by running the following command inside of the directory you created:

```bash
docker run -d --name goodfriend-api --restart unless-stopped --env-file .env -v $(pwd)/logs:/app/logs ghcr.io/bitsofabyte/goodfriend:latest
```

You can forward the port of the container to your host machine by adding `-p PORT:HOST_PORT` to the command above. For example, if you wanted to forward port 8000 from the container to port 80 on your host machine, you would run `-p 8000:80`.

You can also substitute `latest` with a specific version of the API if you want to run a specific version. For example, if you wanted to run version `1.4.2.1`, you would run `ghcr.io/BitsOfAByte/goodfriend:1.4.2.1`. It is recommended you use the `latest` version if you intend to host the API to be used with the newest versions of GoodFriend-compatiable plugins.

### Accessing Logs & Metrics

The API exposes a few endpoints that can be used to access logs and metrics. These endpoints require the `SECURITY_AUTH_TOKEN` from the `.env` file to be passed in the `Authorization` header of the request as a `Bearer` token.

You can find the logs at `/logs` which will return a list of all the log files that are available. You can then access a specific log file by passing the filename in the `file` query parameter, or using the index of the file in the list of files returned by `/logs`.

You can find the metrics at `/metrics` which will return a Prometheus formatted metrics file which can then be used to setup a dashboard with a tool like [Grafana](https://grafana.com/).
