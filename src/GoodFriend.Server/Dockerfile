FROM node:alpine
WORKDIR /api

# Copy & Install Depencencies
COPY ["package.json", "yarn.lock", "./"]
RUN yarn

# Copy project in & build it
COPY . .
RUN ["yarn", "build"]

# Remove all un-needed files for a production build (.ts files, dev-deps, etc.)
RUN ["find", ".", "-type", "f", "-name", "*.ts", "-delete"]
RUN ["rm", "-rf", "node_modules"]
RUN ["find", ".", "-type", "d", "-empty", "-delete"]

# Install only production dependencies
RUN ["yarn", "--production"]

# When the container is run, start the app.
CMD ["node", "."]