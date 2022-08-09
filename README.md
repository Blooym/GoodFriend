<!-- Repository Header Begin -->
<div align="center">

<img src="./.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
### Good Friend
A server/plugin tool for providing friend notifications to players in-game without in-game polling.

[![Download Count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend)](https://github.com/BitsOfAByte/GoodFriend) 
[![Latest Release](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=blue&label=Release)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)
[![Latest Preview](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=orange&include_prereleases&label=Testing)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)

**[Issues](https://github.com/BitsOfAByte/GoodFriend/issues) · [Pull Requests](https://github.com/BitsOfAByte/GoodFriend/pulls) · [Releases](https://github.com/BitsOfAByte/GoodFriend/releases/latest)**

</div>

---
<!-- Repository Header End -->

## About
GoodFriend is a plugin written for the [Dalamud](https://github.com/goatcorp/Dalamud) plugin framework which allows players to recieve notifications when their in-game friends login and logout, as this feature is not provided natively.

This repository is split into two sub-components, the [Server](src/GoodFriend.Server/) component and the [Plugin](src/GoodFriend.Plugin/) component. More information about these sub components can be found inside of the their respective README.md files within their folder.

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
- The official API instance is, in most definitions, stateless. It does not retain any information about requests once they have been processed in any log file or database. The only place this is visible is STDOUT, which can be disabled.
- The official API instance enforces a strict ratelimit on all requests to prevent abuse and ensure that any malicious actors cannot cause major issues.
- The official API instance only allows communication over HTTPs and ignores all HTTP traffic. 
- The Client will only listen for and send updates when strictly necessarily (eg. the Client will not be connected to the API when not logged in)
- The Client (by default) will also use the GUID of the assembly as additional salt to make sure it is communicating with the same version (strict mode only)

## Contributions
Contribution rules are a lot tighter on this project than most to help increase security of all the components involved. I am currently not accepting additional features that would require tweaks to the server component UNLESS they are strictly for increasing user security and privacy. If you have any concerns about security or privacy please open an issue and I will help resolve it the best way I can.
