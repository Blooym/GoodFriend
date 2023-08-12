# API

## Selfhosting

You can self-host the API either by building the binary yourself using the Rust toolchain or by using the prebuilt docker image available on the GitHub Container Registry. The recommended method is to use the docker image, as it is much easier to set up and maintain, especially as future versions are released.

### Initial Setup & Configuration

Before proceeding, you need to create a `config.toml` file within a new directory named `data`. This directory will store any persistent files required by the API for access or storage. For now, it only holds the configuration file. If you want to specify a different location for the configuration file, you can set the `CONFIG_FILE_LOCATION` to the path of your `config.toml` file. Next, fill out the configuration file using the example provided [here](./data/config.toml.example).

Additionally, you can provide environment variables supported by [Rocket](https://rocket.rs/v0.5-rc/guide/configuration/#environment-variables) to the API when running it, allowing you to modify aspects such as the port it runs on or its logging level.

Keep in mind that you'll need to run the API behind a reverse proxy like [NGINX](https://www.nginx.com/) or [Caddy](https://caddyserver.com/) to handle TLS, as the API does not support it natively.

#### Docker

Using Docker is the simplest way to begin with the API. Run the latest image from the GitHub Container Registry [image](https://github.com/Blooym/GoodFriend/pkgs/container/goodfriend) by executing `docker run -d -p 8000:8000 -v /path/to/data:/app/data ghcr.io/blooym/goodfriend:latest`. This command will initiate the API on port 8000 and link the `/path/to/data` directory to the `/app/data` directory inside the container.

#### From Source

To build the API from source, you must install the Rust stable toolchain. Follow the instructions provided [here](https://www.rust-lang.org/tools/install) to achieve this. Once the toolchain is installed, build the API by executing `cargo build --release` within the repository's root directory. This process will build the API and place the binary in `target/release/goodfriend-api`. You can then run the API with `./target/release/goodfriend-api`, provided you have completed the initial setup and configuration.
