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

# Build api binary.
COPY . .
ARG RUSTC_APP_FLAGS=--release --bin
RUN cargo build ${RUSTC_APP_FLAGS}

# -----------
#   RUNTIME  
# -----------
FROM gcr.io/distroless/static-debian11:nonroot as runtime
WORKDIR /app

# Bind to all interfaces.
ENV ROCKET_ADDRESS=0.0.0.0 

# Grab api binary from build.
COPY --from=build /build/target/release/goodfriend-api .

# Run the api.
CMD ["./goodfriend-api"]