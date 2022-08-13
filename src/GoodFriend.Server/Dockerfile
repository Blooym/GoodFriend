FROM node:alpine

# Setup work directory for the app.
ENV WORKDIR /api
WORKDIR ${WORKDIR}

# Copy dependency files and install dependencies. 
COPY ["package.json", "yarn.lock", "./"]
RUN yarn

# Copy app files run build.
COPY . .
RUN ["yarn", "build"]

# Remove all non=production files.
RUN ["rm", "-rf", "node_modules"]
RUN ["find", ".", "-type", "f", "-name", "*.ts", "-delete"]
RUN ["find", ".", "-type", "d", "-empty", "-delete"]

# Reinstall dependencies in production mode.
RUN ["yarn", "--production"]
RUN ["rm", "tsconfig.json", "yarn.lock"]

# Create a user for the container.
ENV USER prod
RUN adduser --disabled-password --gecos "" -H ${USER}

# Give the user access to the workdir and lock down permissions.
RUN chmod -R 550 ${WORKDIR}
RUN mkdir -p ${WORKDIR}/logs -m 770
RUN chown -R ${USER}:${USER} ${WORKDIR}

# Create a volume for the logs.
VOLUME ${WORKDIR}/logs

# Set the user and entry command.
USER ${USER}
CMD ["node", "."]