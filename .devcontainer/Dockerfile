FROM ghcr.io/blooym/docker-dalamud:net8-alpine

RUN apk add --update openssh git build-base bash
RUN adduser --disabled-password --gecos "" dev
USER dev
RUN curl https://sh.rustup.rs -sSf | sh -s -- -y