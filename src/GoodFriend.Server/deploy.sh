#!/bin/bash

echo "Beginning deployment, fetching repository..."
git fetch

if [ ! "$(docker ps -a | grep gf-api)" ]; then
    echo "You don't have a running container, starting one..."
    git pull
    docker-compose up -d --build
    exit 0
fi


if [ "$(git diff --name-only origin/main)" ]; then
        echo "You have a running container and the repository has changed, pulling"
        git pull
        echo "Rebuilding containers with latest changes"
        docker-compose up -d --build
        echo "Good to go."
        exit 0
else
    echo "No repository changes, no need to deploy anything, exiting."
    exit 0
fi