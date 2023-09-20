FROM ghcr.io/blooym/docker-dalamud:stg-alpine

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

RUN apk add --update openssh git build-base bash

RUN adduser --disabled-password --gecos "" dev
USER dev

RUN curl https://sh.rustup.rs -sSf | sh -s -- -y