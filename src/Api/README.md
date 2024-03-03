# API

This is the API that GoodFriend uses to relay information between users without making any calls to the FFXIV servers.

## Running

You can run the API either by building it manually using the Rust toolchain or by using the pre-built Docker images available on the GitHub Container Registry at `ghcr.io/blooym/goodfriend`.

It is recommended that you use docker image as it is much easier to set up, maintain, and update long-term as future versions are released.

### Configuration

Before proceeding, you need to create a `config.toml` somewhere that the API can access. This file will store all of the configuration information for the API and is automatically hot-reloaded at runtime when changes are detected without having to restart the entire process. By default the configuration will be located at `./data/config.toml`, but this path can be changed either with the `--config` flag or the `GOODFRIEND_CONFIG=` environment variable.

An example configuration is available [here](./data/config.toml.example) with documentation for most configuration options.

You can also configure the underlying Rocket server by any way that Rocket supports, you can learn more about this [here](https://rocket.rs/v0.5/guide/configuration/#environment-variables).

#### Changing SSE stream capacity

By default the API has a server-sents events capacity of `5000`, which is the ideal and recommended value for most users. However, you may wish to decrease or increase this value due to factors like memory usage or your instance reaching it's capacity.

To increase the server-sent event stream capacity for an endpoint, you can pass the following flags or their environment variable equivilants at startup:

- `--api-player-sse-cap <PLAYER_SSE_CAP>` (env: `GOODFRIEND_API_PLAYERSSE_CAP=`)
- `--api-announce-sse-cap <ANNOUNCE_SSE_CAP>` (env: `GOODFRIEND_API_ANNOUNCESSE_CAP=`)

### Prometheus

You can enable Prometheus metrics by running the `--api-enable-metrics` flag at startup. When enabled, they will be available at `/metrics` (please keep in mind you will need a reverse proxy if you want to add authentication to this endpoint).

#### Docker

Using Docker is the simplest way to host the API. To get started, pull and run the latest image from the [GitHub Container Registry](https://github.com/Blooym/GoodFriend/pkgs/container/goodfriend) by executing `docker run -d -p 8000:8000 -v /path/to/data:/app/data ghcr.io/blooym/goodfriend:latest`. This command will start the API on port 8000 and bind the `/path/to/data` directory to the `/app/data` directory inside the container to persist the configuration across restarts.

#### From Source

To build the API from source, you must install the Rust stable toolchain, you can find more information on how to do this [here](https://www.rust-lang.org/tools/install). 

Once Rust is installed you can build the project by running `cargo build --release` in the Api directory. You can then run the binary at `./target/release/goodfriend-api`  provided you have completed the initial setup and configuration.
