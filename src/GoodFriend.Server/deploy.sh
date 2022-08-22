#!/bin/bash

REPO_BRANCH=main

echo "Beginning deployment, fetching repository..."

# Fetch changes from remote repository.
git fetch
if [ $? -ne 0 ]; then
    exit 1
fi

# If there is no running container, pull and start one up.
if [ ! "$(docker ps -a | grep gf-api)" ]; then
    echo "You don't have a running container, starting one..."
    git pull
    docker-compose up -d --build
    exit 0
fi

# If there is a running container and the remote has changed, pull and rebuild then restart.
if [ "$(git rev-list HEAD...origin/$REPO_BRANCH --count)" -gt 0 ]; then
        echo "You have a running container and the repository has changed, pulling"

        # Rebuild on success, error if a pull fails. 
        git pull
        if [ $? -eq 0 ]; then
            echo "Pull successful, restarting container..."
            docker-compose up -d --build
            exit 0
        else
            echo "Pull failed, exiting..."
            exit 1
        fi

        # Successful pull! We're done.     
        echo "Good to go."
        exit 0
else
    # No changes on the remote, no need for action.
    echo "No repository changes, no need to deploy anything, exiting."
    exit 0
fi