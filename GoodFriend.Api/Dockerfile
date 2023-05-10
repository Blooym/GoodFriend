# -----------
#    BUILD   
# -----------
FROM rustlang/rust:nightly-alpine as build
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

# Grab api binary from build.
COPY --from=build /build/target/release/ .

# Run the api.
CMD ["./goodfriend-api"]
