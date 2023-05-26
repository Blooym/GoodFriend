FROM ghcr.io/bitsofabyte/docker-dalamud:staging

# Optout of telemtry.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Install required dependencies.
RUN apk add --update openssh git curl build-base

# Create a non-root dev user.
RUN adduser --disabled-password --gecos "" dev

# Rust Nightly.
USER dev
RUN  curl https://sh.rustup.rs -sSf | sh -s -- -y
USER root