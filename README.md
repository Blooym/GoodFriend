<!-- Repository Header Begin -->
<div align="center">

<img src="./.assets/icon.png" alt="Goodfriend Logo" width="15%">
  
### Good Friend
A server/plugin tool for providing friend notifications to players in-game without in-game polling.

[![Download Count](https://img.shields.io/endpoint?url=https://vz32sgcoal.execute-api.us-east-1.amazonaws.com/GoodFriend&label=Plugin%20Downloads)](https://github.com/BitsOfAByte/GoodFriend)
[![Online Users](https://img.shields.io/badge/dynamic/json?url=https://aether.bitsofabyte.dev/v2/clients&label=API%20Online%20Users&query=clients&colour=green)](https://github.com/BitsOfAByte/GoodFriend)
[![Crowdin](https://badges.crowdin.net/goodfriend/localized.svg)](https://crowdin.com/project/goodfriend)
[![Latest Release](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=blue&label=Release)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)
[![Latest Preview](https://img.shields.io/github/v/release/BitsOfAByte/GoodFriend?color=orange&include_prereleases&label=Testing)](https://github.com/BitsOfAByte/GoodFriend/releases/latest)

**[Issues](https://github.com/BitsOfAByte/GoodFriend/issues) · [Pull Requests](https://github.com/BitsOfAByte/GoodFriend/pulls) · [Releases](https://github.com/BitsOfAByte/GoodFriend/releases/latest)**

</div>

---

<!-- Repository Header End -->

## About

GoodFriend is a plugin written for the [Dalamud](https://github.com/goatcorp/Dalamud) plugin framework which allows players to supercharge the in-game friendslist capabilities and do things such as recieve notifications when their friends login and logout.

This is achieved by utilising a custom built API to work around some of the in-game limitations regarding friend data; essentially acting as a "relay" server between clients to protect individual privacy. Due to the nature of this design, most of the functionality provided by the plugin can only be utilised if both sides (eg. another friend) is using the plugin.

## Installing

This plugin can be downloaded from the official Dalamud plugins repository in-game by navigating to the plugin installer and searching for "Good Friend". Once installed you will be automatically connected to the official API instance and be ready to go without the need for any additional configuration.

## 3rd-party API Instances

This plugin allows for 3rd party API instances to be used instead of the official instance provided here. It is highly recommended you stick to the official instance as any 3rd party instance could potentially be dangerous, either by being insecure or actively abusing security vulnerabilities. 

## Technical Details
**"Client"** refers to a user that is running the plugin.
**"Server"** refers to the given API instance
**"Clients"** refers to all users of the given API instance.

A general flow of how this plugin would provide a login notification is as follows:
1. **Client** subscribes the server event stream to recieve all events for other users.
2. **Client** logs into a character, the plugin hashes their ID and sends a PUT request to the **server** with the relevant information.
3. **Server** distributes the information recieved from the PUT request to all **clients** listening for events.
4. **Clients** will recieve the information and attempt to match all friends' hashes against the recieved ContentID.
5. **Clients** that successfully match the ID to a friend will then display a notification for the event.

### Security & Privacy Considerations

Security and privacy was greatly considered during design and as such both the plugin and the server implement some methods to ensure the safety and privacy of all users, some examples are:

- All information that could be used to link a client back to an in-game player is hashed before being sent to the server.
- The plugin uses additional salt such as the assembly GUID (by default) and a user-entered friend code to make the hash more unpredicable by the server to prevent rainbow tables of ContentIDs
- The official server instance enforces strict ratelimiting to prevent any spammed abusive requests. 
- The official server is secured behind HTTPs and a reverse proxy.
- The plugin will automatically disconnect from the API event stream(s) when it is not strictly necessary to be connected.
- The plugin only transmits essential information for the server to process.
- The server tries to keep logs of all inbound requests to a minimum where possible, and automatically performs any cleanup of old logs after a certain amount of time.

## Contributions

### Code Changes
Contributions are welcome for the plugin as long as they remain in-scope and follow the Dalamud rules for submission. It is preferred you open an issue beforehand if you wish to work on something.

If you would like to setup a development environment it is preferred, but not required, to use the provided [Dockerfile](/.devcontainer/Dockerfile) and [development container](/.devcontainer/devcontainer.json) configuration with a compliant tool which will automatically handle installing all the required dependencies for you and deploy a development API to use to test the plugin against.

### Translation & Localizations
If you wish to contribute localizations to this project, please do so over on the project [Crowdin](https://crwd.in/goodfriend).
