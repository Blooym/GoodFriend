# -------------------
# --  Build Stage  --
# -------------------
FROM node:lts-alpine as build
WORKDIR /build

# Install global build dependencies
RUN npm i -g pkg


# Install dev dependencies & transpile typescript to javascript
COPY ["package.json", "yarn.lock", "./"]
RUN yarn install --production=false
COPY . .
RUN yarn build


# Build a production executable.
RUN rm -rf node_modules
RUN yarn install --production=true
RUN pkg -C Brotli -t node16-alpine-x64 ./dist/index.js -o server \
    && chmod +x ./server



# -------------------
# -- Runtime Stage --
# -------------------
FROM alpine:latest as runtime
WORKDIR /app

# Copy over the executable and import env variables.
COPY --from=build /build/server .
COPY .env* ./


# Prepare a user to run the app as.
ENV USER prod
RUN adduser --disabled-password --gecos "" -H ${USER}


# Hand over ownership to the user.
RUN chmod -R 550 .
RUN mkdir -p ./logs -m 777
RUN chown -R ${USER}:${USER} .


# Remove chmod and chown from the image to prevent overrides.
RUN rm /bin/chmod /bin/chown


# Set the user and entry command.
USER ${USER}
CMD ["./server"]