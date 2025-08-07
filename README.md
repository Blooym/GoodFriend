<div align="center">

<img src="./assets/icons/icon.png" alt="GoodFriend Logo" width="15%">
  
### GoodFriend

A Dalamud plugin & associated web-API that provides improved in-game social functionality 

[![Plugin Downloads](https://img.shields.io/endpoint?url=https://dalamud-dl-count.blooym.workers.dev/GoodFriend&label=Plugin%20Downloads)](https://github.com/Blooym/Dalamud.GoodFriend)
[![Crowdin Localization](https://badges.crowdin.net/goodfriend/localized.svg)](https://crowdin.com/project/goodfriend)
[![Licence](https://img.shields.io/github/license/Blooym/Dalamud.GoodFriend?color=blue&label=Licence)](https://github.com/Blooym/Dalamud.GoodFriend/blob/main/LICENSE)

**[Issues](https://github.com/Blooym/Dalamud.GoodFriend/issues) · [Pull Requests](https://github.com/Blooym/Dalamud.GoodFriend/pulls) · [Releases](https://github.com/Blooym/Dalamud.GoodFriend/releases/latest)**

</div>

---

## About
GoodFriend is divided into three components:

- [Plugin](./src/Plugin): Interacts with the API on behalf of the user and automatically handles any interactions. Also includes functionality that does not interact with the API and, as such, can be used standalone.

- [Client](./src/Client): A usage-agnostic client library that wraps API calls and includes things like an SSE client and automatic request serialization/deserialization. This library is used by the plugin to interact with the API but is not directly tied to Dalamud so can, in theory, can be used by any application (although it's not recommended).

- [API](./src/Api): A REST API that manages the actual communication between clients, as well as various other tasks. By default, the provided plugin will use the official API instance, but this can be changed to any other instance if desired.

### Drawbacks

Due to the nature of the implementation, only users with the plugin installed can send and receive events with the API, as the plugin is responsible for handling the actual event sending and receiving. This means that if you have the plugin installed but your friend does not, any functionality that depends on receiving data from them will not work. Unfortunately, the only way to address this is to ask your friends to install GoodFriend.

## Translation & Localization

If you want to contribute localizations to this project please visit GoodFriends's [Crowdin page](https://crwd.in/goodfriend).
