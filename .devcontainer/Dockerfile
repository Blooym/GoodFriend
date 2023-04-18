FROM ghcr.io/bitsofabyte/docker-dalamud:latest

# Optout of telemtry.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Create a non-root dev user.
RUN adduser --disabled-password --gecos "" dev

# Install required dependencies.
RUN apk add --update openssh git curl build-base

# Rust Nightly.
USER dev
RUN  curl https://sh.rustup.rs -sSf | sh -s -- --default-toolchain nightly -y
USER root