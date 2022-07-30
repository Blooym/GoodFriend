<!-- Repository Header Begin -->
<div align="center">

<img src="./.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
### Good Friend
A plugin/server tool for providing friend notifications to players.
  
</div>

---
<!-- Repository Header End -->

## About
This repository is split into 2 sub-components, the [Server](src/GoodFriend.Server/) and the [Plugin](src/GoodFriend.Plugin/). More information about the specified components can be found inside of their respective README.md files.

The plugin communicates status updates with the server via a PUT request and recieves status updates from the server using Server-Sent Events, this allows for information to be sent to other players without using any in-game packets or commands.

The general flow of the plugin is as follows:

1. Player logs in to a character
2. Plugin sends a PUT request to the server with a hashed version of the player's ContentID and login status
3. Plugin starts listening for status updates from the server via SSE
3. Server handles the event and sends the update to all plugins that are listening for it
4. Client recieve the update and compare it against hashes of their friends' ContentIDs
5. Client plugin will display a notification if it finds a match


## Security Considerations
The security of player information was highly considered whilst designing this tool, the following things have been implemented to ensure higher privacy & security:

- All ContentIDs are hashed before being sent to the server, and the player can add additional salt for "closed circle" notifications
- The server is essentially stateless, it does not retain any information about the request or clients after sending it to clients
- The server enforces strict ratelimiting to ensure that the API cannot be spammed with updates
- The official deployment uses HTTPs for all communication, and the server component is designed to use HTTPs on production builds
- The plugin only listens for updates when the player is logged in, preventing any excess inbound traffic from the server

## Caveats
Due to the nature of this plugin, it is only able to recieve updates from other plugin users. This means that in order to recieve notifications for a friend they will also have to be running the plugin to communicate updates to the server. 

Unfortunately, due to the way the in-game friends list system is designed, this is currently not able to be resolved as the friends list is only loaded when logging in, exiting a duty or whenever the social menu is opened.

## Installing
This plugin is not on the official DalamudPlugins repository as of now, and depending on the review process may never be fully available. 

If you wish to download it from the 3rd party repository, please add the following link to your 3rd-party repository list in the Dalamud settings.

```
https://raw.githubusercontent.com/BitsOfAByte/GoodFriend/main/repo.json
```

Please note that if you decide to use the 3rd party repository, you will **not** recieve any support from Dalamud support channels.


## Contributions
Contributions are welcomed if they help increase the security of either the Server or the Plugin or optimise how any part of the process works. Please note that new features are not currently planned and an issue should be raised requesting it before spending time developing anything.
