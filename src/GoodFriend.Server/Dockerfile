# Set the image to use
FROM node:alpine

# Set up a folder for the app
WORKDIR /api

# Copy the app into the container
COPY . .

# Install dependencies
RUN yarn

# This port will take precedence over the port in .env
ENV PORT=8080
EXPOSE ${PORT}

# Run the app
CMD ["yarn", "start"]