
# -------------------
# --  Build Stage  --
# -------------------
FROM node:alpine as build
WORKDIR /build

# Install dev dependencies and install them.
COPY ["package.json", "yarn.lock", "./"]
RUN yarn install

# Copy app files and build.
COPY . .
RUN ["yarn", "build"]



# -------------------
# -- Runtime Stage --
# -------------------
FROM node:alpine as runtime
WORKDIR /app

# Install production dependencies.
COPY ["package.json", "yarn.lock", "./"]
RUN ["yarn", "--production"]

# Copy over build files from build stage and environment files.
COPY --from=build /build/dist .
COPY .env .

# Create a user and group for the app.
ENV USER prod
RUN adduser --disabled-password --gecos "" -H ${USER}

# Give the user access to the workdir and lock down permissions.
RUN chmod -R 550 .
RUN mkdir -p ./logs -m 777
RUN chown -R ${USER}:${USER} .

# Remove chmod/chown to prevent permission overriding.
RUN rm /bin/chmod /bin/chown

# Set the user and entry command.
USER ${USER}
CMD ["node", "."]