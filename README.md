<!-- Repository Header Begin -->
<div align="center">

<img src="./.assets/icon.png" alt="GoodFriend Logo" width="15%">
  
### GoodFriend
A server & plugin for sending/recieving login & logout notifications with friends.

[![Download Count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend&label=Plugin%20Downloads)](https://github.com/BitsOfAByte/GoodFriend)
[![Crowdin](https://badges.crowdin.net/goodfriend/localized.svg)](https://crowdin.com/project/goodfriend)
[![Latest Release](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=blue&label=Release)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)
[![Latest Preview](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=orange&include_prereleases&label=Testing)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)

**[Issues](https://github.com/BitsOfAByte/GoodFriend/issues) · [Pull Requests](https://github.com/BitsOfAByte/GoodFriend/pulls) · [Releases](https://github.com/BitsOfAByte/GoodFriend/releases/latest)**

</div>

---

<!-- Repository Header End -->

## About

GoodFriend is a [plugin](./GoodFriend.Plugin/) & [server](./GoodFriend.Server/) tool that allows for sending and recieving friend information such as login/logout notifications without the need for in-game polling, achieved by utilising a custom-built API to work around some of the in-game limitations regarding friend data; essentially acting as a "relay" server between clients to protect individual privacy. Due to the nature of this design, most of the functionality provided by GoodFriend can only by friends who both have the plugin installed.

The API is easily self-hostable with little work needed to get it going, just in-case anything ever happens to the official instance. You can find out more about the API [here](./GoodFriend.Server/)

### Features

- Quick friend notifications between GoodFriend users.
- Event log for viewing server events and other information for transparency and debugging.
- Powerful filtering options for customising what events are sent to you.
- Easy to [selfhost](./GoodFriend.Server/) API with Docker.
- Privacy-focused design with unidentifiable event relaying.
- Authentication support (bearer, basic) built into client.
- Localisation support for the entire plugin.
- Easy to use user interface.
- Multiple notification types, like in-game chat messages or toast notifications.

### Screenshots

<img src="./.assets/Screenshots/screenshot1.png" alt="GoodFriend Screenshot 1" width="32%"> <img src="./.assets/Screenshots/screenshot2.png" alt="GoodFriend Screenshot 2" width="32%"> <img src="./.assets/Screenshots/screenshot3.png" alt="GoodFriend Screenshot 3" width="32%">

## Installing

[GoodFriend.Plugin](./GoodFriend.Plugin/) is available from the official Dalamud plugin repository which is accessible in-game by opening the Dalamud Plugin Installer and searching for "GoodFriend". Once installed, you will be automatically connected to the default (official) API instance and ready to go without the need for any further configuration.

If you would like to configure any of the settings, you can do so by opening the configuration window and clicking on the "settings" button.

## 3rd-party API Instances / Selfhosting

[GoodFriend.Plugin](./GoodFriend.Plugin/) allows for changing your API instance to any other instance you would like to use. It is highly recommended you use the official instance for privacy and security reasons as any 3rd-party instance may be running insecure/malicious code.

If you still wish to use a 3rd-party instance, you can do so by clicking the settings button in the configuration window, navigating to the "Advanced" tab and changing the "API URL" to the URL of the instance you wish to use and restarting the plugin. As long as the server route version (eg. `/v4/`) matches the APIClient version everything should work properly. If the server is running an incompatible version you may run into issues and no support will be provided from this repository, so you will need to contact the instance owner for support.

If you wish to host your own instance, please read the README in the [GoodFriend.Server directory](./GoodFriend.Server/README.md).

## Technical Details & Privacy Considerations

**Client** refers to a single user that is running the plugin.  
**Clients** refers to all users connected to a given instance.
**Server** refers to a web-accessible instance of the API.

A general overview of how GoodFriend works is as follows:

1. A **client** logs into a character, the plugin hashes their ID using a configured salt type, generates an additional salt and sends a PUT request to the **server** with the event information and the generated salt.
2. The **client** then subscribes the server-sent event stream to receive all events for other users that are currently connected to the **server**.
3. The **Server** distributes the information received from the PUT request to all **clients** listening to the server-sent event stream.
4. All **clients** will receive the information and attempt to match all in-memory friend hashes against the received ContentID hash using the generated salt and also the configured salt type.
5. All **clients** that successfully match the hashed ContentID to a friend will then display a notification for the event using information from the game memory, preventing sending this information through the **server**.

Key Security implementations:

- Identifying information about a client is hashed before being sent to the server.
- Additional salt is generated for each request to prevent pre-computing hashes.
- The client will only accept data from the server that matches the expected format.
- The server enforces strict rate-limiting to mitigate spam and abusive requests.
- The official server is hosted in a secure environment with strict security policies.
- The plugin automatically disconnects from the server when the connection is unnecessary.
- All interactions with the server are stored in the plugin event log for transparency.
- The server will not accept any data from a client if it does not match the expected format.

## Translation & Localization

If you wish to contribute localizations to this project, please do so over on the project [Crowdin](https://crwd.in/goodfriend). If the language you wish to translate is not available, please create an issue and it will be added.
