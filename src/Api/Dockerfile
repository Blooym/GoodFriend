# ----------
#    USER
# ----------
FROM alpine:latest as user
RUN adduser -s /bin/false -D user

# -----------
#    BUILD
# -----------
FROM rust:1-alpine as build
WORKDIR /build

# Install essential build tools.
RUN apk add --no-cache --update build-base

# Pre-cache cargo dependency builds.
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
FROM scratch as runtime
WORKDIR /app

# Import and switch to non-root user.
COPY --from=user /etc/passwd /etc/passwd
COPY --from=user /bin/false /bin/false
USER user


# Bind to all interfaces to allow access from outside the container.
ENV ROCKET_ADDRESS=0.0.0.0 
ENV ROCKET_PORT=8000
ENV GOODFRIEND_DATA_DIRECTORY=/app/data
EXPOSE 8000

# Grab API binary from build.
COPY --from=build /build/target/release/goodfriend-api /app/bin/

# Run the api.
ENTRYPOINT ["/app/bin/goodfriend-api"]
