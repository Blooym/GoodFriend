<div align="center">

<img src="./.assets/icon.png" alt="GoodFriend Logo" width="15%">
  
### GoodFriend

A Dalamud plugin & associated web-API that provides improved in-game social functionality 

[![Latest Version](https://img.shields.io/github/v/release/Blooym/GoodFriend?color=blue&label=Version)](https://github.com/Blooym/GoodFriend/releases/latest)
[![Plugin Downloads](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend&label=Plugin%20Downloads)](https://github.com/Blooym/GoodFriend)
[![Connected Users](https://img.shields.io/endpoint?url=https://gf-clients.blooym.workers.dev/&label=Connected%20Users)](https://github.com/Blooym/GoodFriend)
[![Crowdin Localization](https://badges.crowdin.net/goodfriend/localized.svg)](https://crowdin.com/project/goodfriend)
[![Licence](https://img.shields.io/github/license/Blooym/Wholist?color=blue&label=Licence)](https://github.com/Blooym/Wholist/blob/main/LICENSE)

**[Issues](https://github.com/Blooym/GoodFriend/issues) · [Pull Requests](https://github.com/Blooym/GoodFriend/pulls) · [Releases](https://github.com/Blooym/GoodFriend/releases/latest)**

</div>

---

## About

GoodFriend is split into **three** components:

1. **[Plugin](./src/Plugin/)**: Interacts with the API on behalf of the user and automatically handles any interactions. The plugin is designed in a modular way that allows for the easy addition of new functionality that can be enabled/disabled/configured by the user easily. Not all modules interact with the API and can be used without any connection.

2. **[Client](./src/Client/)**: A usage-agnostic client library that handles interacting with the API directly and handles automatic reconnection, serialization/deserialization, authentication, etc. This library is used by the plugin to interact with the API but is not tied directly to Dalamud and can, in theory, be used by any application.

3. **[API](./src/Api/)**: A web REST API that handles the actual communication between clients, as well as various other tasks. The API is designed to be easy self-hostable and lightweight as possible while still providing everything needed for the plugin to work. By default the plugin will use the official API instance, but this can be changed to any other instance if desired.

### Why?

This plugin was initially created as a way to get around the limitations of the in-game friend system as it was unable to provide notifications when people on my friends list logged in or out of the game. It has since been redesigned to allow for additional functionality to be added without much additional groundwork.

### Drawbacks

Due to the nature of the implementation only users with the plugin installed can send and recieve events with the API as the plugin is responsible for handling the actual event sending & recieving. This means that if you have the plugin installed but your friend does not, you will not recieve any events from them and vice-versa. Unfortunately, the only way to get around this is to ask your friends that you want to recieve events from to install the plugin.

### Features

- Modular design with customizable and toggleable modules, enable only the functionality you want.
- Powerful filtering options for customising what events are sent to you.
- Easy to [selfhost](./src/Api/) API, a pre-built docker image is available on GitHub Container Registry for amd64 and arm64.
- Privacy-focused design from end-to-end, no identifiable information is ever available about you when sent to other users.
- & more that I definitely forgot to mention, probably.

## Installing

The plugin is available to install from the official Dalamud Plugin Repository, accessible in-game by opening the Dalamud Plugin Installer and searching for "GoodFriend". Once installed, you will be automatically connected to the default (official) API instance and ready to go! If you wish to do any further configuration, simply open up the plugin window and look around.

## Selfhosting & 3rd-party Instances

The web API has been designed to be easy to self-host and is available as a pre-built docker image on GitHub Container Registry [here](https://github.com/Blooym/GoodFriend/pkgs/container/goodfriend). No support is provided for self-hosted instances, but if you run into any issues with the API itself please open an issue and I will try to help as best I can.

If you wish to host your own instance, please read the README in the [API Directory](./src/Api/README.md).

## Translation & Localization

If you wish to contribute localizations to this project, please do so over on the project [Crowdin](https://crwd.in/goodfriend). If the language you wish to translate is not available, please create an issue and it will be added.
