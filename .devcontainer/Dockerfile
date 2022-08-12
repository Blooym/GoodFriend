FROM mcr.microsoft.com/dotnet/sdk:latest
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Create the user for the container.
RUN adduser --disabled-password --gecos "" dev

# Install Required Dependencies for this project.
RUN apt update && apt install -y apt-transport-https ca-certificates curl gnupg-agent software-properties-common
RUN curl -fsSL https://download.docker.com/linux/debian/gpg | apt-key add -
RUN add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable"
RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash -
RUN apt install -y unzip git nodejs openssh-\*
RUN npm i --location=global yarn npm

# Download the latest stable version of Dalamud and extract it, will be overriden by devcontainer.json if set to do so.
WORKDIR /dependencies/dalamud
ENV DALAMUD_HOME=/dependencies/dalamud
RUN wget -O dalamud.zip https://goatcorp.github.io/dalamud-distrib/latest.zip
RUN unzip dalamud.zip
RUN rm dalamud.zip

# Download the project into the workspace 
WORKDIR /workspaces/GoodFriend 
RUN git clone https://github.com/BitsOfAByte/GoodFriend.git