<!-- Repository Header Begin -->
<div align="center">

<img src="./.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
### Good Friend
A server/plugin tool for providing friend notifications to players in-game without in-game polling.

[![Download Count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend&label=Plugin%20Downloads)](https://github.com/BitsOfAByte/GoodFriend)
[![Latest Release](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=blue&label=Release)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)
[![Latest Preview](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=orange&include_prereleases&label=Testing)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)

**[Issues](https://github.com/BitsOfAByte/GoodFriend/issues) · [Pull Requests](https://github.com/BitsOfAByte/GoodFriend/pulls) · [Releases](https://github.com/BitsOfAByte/GoodFriend/releases/latest)**

</div>

---

<!-- Repository Header End -->

## About

GoodFriend is a plugin written for the [Dalamud](https://github.com/goatcorp/Dalamud) plugin framework which allows players to recieve notifications when their in-game friends login and logout, as this feature is not provided natively.

This repository is split into two sub-components, the [Server](src/GoodFriend.Server/) component and the Plugin component. More information about these sub components can be found inside of the their respective README.md files within their folder.

## Caveats

Due to the nature of how this plugin is designed it can only recieve status updates from other users of the same API. This means, in essence, you will only recieve in-game notifications for players both use a compatible plugin for the API and share the same "friend code" as you.

Unfortunately, due to the way the in-game friends list system is designed this is cannot not changed as the friends list is only loaded when logging in, exiting a duty or whenever the social menu is opened, and polling frequently would cause more issues then it is worth for recieving friend notifications.

## Installing

This plugin is not on the official DalamudPlugins repository as of right now and is pending a review, and it is highly recommended to wait for this process to be complete. If you wish to download it from the 3rd party repository anyway, add the following link to your 3rd-party repository list in the Dalamud settings.

```
https://raw.githubusercontent.com/BitsOfAByte/GoodFriend/main/repo.json
```

Please note that if you decide to use the 3rd party repository, you will **not** recieve any support from Dalamud support channels and should not ask for any help.

## 3rd-party API Instances

It is **highly. highly recommended** you ONLY Use the official API instance that is configured, as changing this to an untrusted 3rd party URL could be a security and privacy risk. If you wish to do so anyway then it is recommended to make use of the "Friend Code" system to further your own privacy. **NEVER** connect to an API that does not use strict HTTPS for all connections unless you are testing locally.

## Technical Details

**"Client"** refers to a user that has the plugin installed and **"Server"** refers to the configured API instance and **"Clients"** refer to all users sharing that same instance.

A general flow of how this plugin interacts with the API is as follows:

1. The Client logs into a character in-game and sends a PUT request to the server with their hashed ContentID and Status (LoggedIn: `bool`)
2. The Client sends a GET request to the server to start recieving a stream of Server-Sent Events
3. The Server distributes the information send inside of the initial PUT request to all listing clients.
4. The Clients will then recieve this information and attempt to match their friends' hashed ContentID to the recieved ID.
5. All Clients that find a match will display a notification message of the users choice in-game and cease handling the event.

### Security Considerations

The security of the Client was greatly considered during design and as such both The Client and The Server implemenet some methods to ensure increased security & privacy, of which some things are:

- All personally identifying information (which in this case is the Clients ContentID) is hashed using SHA512 before leaving their machine.
- The user has the option to specify additional salt to use when sending a hash to the server, which will then only notify users using the same salt.
- The official API instance enforces a strict ratelimit on all requests to prevent abuse and ensure that any malicious actors cannot cause major issues.
- The official API instance is HTTPS secured.
- The Client will only listen for and send updates when strictly necessarily (eg. the Client will not be connected to the API when not logged in)
- The Client (by default) will also use the GUID of the assembly as additional salt to make sure it is communicating with the same version (strict mode only)

### Logs & Privacy

When making a request to the official API instance, some things are logged both to the STDOUT and a log file for the sake of finding bugs & preventing abuse. More sensitive information like IP addresses is not retained. You can view the sourcecode for the API log handler [here](src/GoodFriend.Server/Utils/Logger.ts).

## Contributions

Contributions are welcome if they are for one of the following:

- Improving the security of the plugin / server (deployment, vulnerabilites, etc)
- Fixing existing bugs or bad UI/UX design within the plugin.
- Improving performance of existing features
- Helping improve the look and layout of the codebase

As of right now, it is preferred not to make pull requests that add new major features to the plugin as it is aimed to be as minimal as possible.

If you would like to setup a development environment, it is preferred, but not required, to use the provided [Dockerfile](/.devcontainer/Dockerfile) and [development container](/.devcontainer/devcontainer.json) configuration with a compliant tool, as this will install all required dependencies for you and deploy a live-reloading development API to use to test the plugin against.
