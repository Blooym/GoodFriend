<div align="center">

<img src="./assets/icons/icon.png" alt="GoodFriend Logo" width="15%">
  
### GoodFriend

A Dalamud plugin & associated web-API that provides improved in-game social functionality 

[![Latest Version](https://img.shields.io/github/v/release/Blooym/GoodFriend?color=blue&label=Version)](https://github.com/Blooym/GoodFriend/releases/latest)
[![Plugin Downloads](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend&label=Plugin%20Downloads)](https://github.com/Blooym/GoodFriend)
[![Connected Users](https://img.shields.io/endpoint?url=https://gf-clients.blooym.workers.dev/&label=Connected%20Users)](https://github.com/Blooym/GoodFriend)
[![Crowdin Localization](https://badges.crowdin.net/goodfriend/localized.svg)](https://crowdin.com/project/goodfriend)
[![Licence](https://img.shields.io/github/license/Blooym/Wholist?color=blue&label=Licence)](https://github.com/Blooym/GoodFriend/blob/main/LICENSE)

**[Issues](https://github.com/Blooym/GoodFriend/issues) · [Pull Requests](https://github.com/Blooym/GoodFriend/pulls) · [Releases](https://github.com/Blooym/GoodFriend/releases/latest)**

</div>

---

About
GoodFriend is divided into three components:

- [Plugin](./src/Plugin): Interacts with the API on behalf of the user and automatically handles any interactions. The plugin is designed in a modular way that allows for easy addition of new functionality, which can be easily enabled, disabled, or configured by the user. Not all modules interact with the API and can be used without any connection.

- [Client](./src/Client): A usage-agnostic client library that directly interacts with the API and manages automatic reconnection, serialization/deserialization, authentication, etc. This library is used by the plugin to interact with the API but is not directly tied to Dalamud. In theory, it can be used by any application.

- [API](./src/Api): A web REST API that manages the actual communication between clients, as well as various other tasks. The API is designed to be easily self-hosted and as lightweight as possible while still providing everything needed for the plugin to function. By default, the plugin will use the official API instance, but this can be changed to any other instance if desired.

## Why?

This plugin was initially created as a way to circumvent the limitations of the in-game friend system, which couldn't provide notifications when people on my friends list logged in or out of the game. It has since been redesigned to allow for additional functionality to be added without much additional groundwork.

## Drawbacks

Due to the nature of the implementation, only users with the plugin installed can send and receive events with the API, as the plugin is responsible for handling the actual event sending and receiving. This means that if you have the plugin installed but your friend does not, you will not receive any events from them, and vice versa. Unfortunately, the only way to address this is to ask your friends whom you want to receive events from to install the plugin.

## Features

- Modular design with customizable and toggleable modules; enable only the functionality you want.
- Powerful filtering options for customizing which events are sent to you.
- Easy-to-self-host API, a pre-built docker image is available on GitHub Container Registry for amd64 and arm64 architectures.
- Privacy-focused design from end to end; no identifiable information is ever available about you when sent to other users.
- And more that I definitely forgot to mention, probably.

## Installing

The plugin is available for installation from the official Dalamud Plugin Repository, accessible in-game by opening the Dalamud Plugin Installer and searching for "GoodFriend." Once installed, you will be automatically connected to the default (official) API instance and ready to go! If you wish to perform any further configuration, simply open up the plugin window and explore.

## Self-hosting & 3rd-party Instances

The web API has been designed to be easily self-hosted and is available as a pre-built docker image on GitHub Container Registry here. No support is provided for self-hosted instances, but if you encounter any issues with the API itself, please open an issue, and I will do my best to assist.

If you wish to host your own instance, please read the README in the [API Directory](./src/Api/README.md).

## Translation & Localization

If you want to contribute localizations to this project, please do so on the project's [Crowdin page](https://crwd.in/goodfriend). If the language you wish to translate is not available, please create an issue, and it will be added.
