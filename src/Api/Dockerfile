# ----------
#    USER
# ----------
FROM alpine:latest AS user
RUN adduser -S -s /bin/false -D goodfriend

# -----------
#    BUILD
# -----------
FROM rust:1-alpine AS build
WORKDIR /build

# Install essential build tools.
RUN apk add --no-cache --update build-base

# Pre-cache cargo dependencies.
COPY ["Cargo.toml", "Cargo.lock", "./"]
ARG RUSTC_CACHE_FLAGS=--release --lib
RUN mkdir src \
    && echo "// Placeholder" > src/lib.rs \
    && cargo build ${RUSTC_CACHE_FLAGS} \
    && rm src/lib.rs

# Copy source and static files.
COPY static ./static
COPY src ./src

# Build the API binary.
ARG RUSTC_APP_FLAGS=--release --bin
RUN cargo build ${RUSTC_APP_FLAGS}


# -----------
#   RUNTIME
# -----------
FROM scratch AS runtime

COPY --from=user /etc/passwd /etc/passwd
COPY --from=user /bin/false /bin/false
USER goodfriend
ENV ROCKET_ADDRESS=0.0.0.0
ENV ROCKET_PORT=8000
EXPOSE 8000

COPY --from=build /build/target/release/goodfriend-api /usr/bin/

ENTRYPOINT ["/usr/bin/goodfriend-api"]
