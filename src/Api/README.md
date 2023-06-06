# GoodFriend API

## Selfhosting

The API can be selfhosted by either building the binary yourself with the Rust toolchain or by using the prebuilt docker image available on the GitHub Container Registry. The recommended method is to use the docker image as it is much easier to set up and maintain as future versions are released.

### Initial Setup & Configuration

Before doing anything you'll need to create a `config.toml` file inside of a new directory called `data`; This directory will house any persistent files that the API needs to access or store, but for now only holds the configuration file. If you wish to specify a different location for the configuration file you can set the `CONFIG_FILE_LOCATION` to the path of your `config.toml` file. Afterwards fill out the configuration file from the example available [here](./data/config.toml.example).

You can also provide environment variables that are supported by [Rocket](https://rocket.rs/v0.5-rc/guide/configuration/#environment-variables) to the API when running it to change things like the port it runs on or the TLS configuration (the binary has been compiled to support TLS).

#### Docker

Running with docker is the easiest way to get started with the API. You can run the latest image from the GitHub Container Registry [image](https://github.com/BitsOfAByte/GoodFriend/pkgs/container/goodfriend) by running `docker run -d -p 8000:8000 -v /path/to/data:/app/data ghcr.io/bitsofabyte/goodfriend:latest`. This will start the API on port 8000 and mount the `/path/to/data` directory to the `/app/data` directory inside of the container. 

#### From Source

To build the API from source you'll need to install the Rust stable toolchain. You can do this by following the instructions available [here](https://www.rust-lang.org/tools/install). Once you have the toolchain installed you can build the API by running `cargo build --release` inside of the root directory of the repository. This will build the API and place the binary in `target/release/goodfriend-api`. You can then run the API by running `./target/release/goodfriend-api` as long as you have completed the initial setup and configuration.