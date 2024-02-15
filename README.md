# Infra's Simple Discord Chat Relay for CounterStrikeSharp

Simple CounterStrikeSharp plugin that logs chats to Discord via webhooks. Just a CS2 equivalent for my previous SourceMod relay: https://github.com/1zc/SM-Discord-Chat 

## Webhook Styles:

The plugin features two webhook styles, one super simple style suited for logging and the other looking slighly prettier. Styles can be configured in `addons/counterstrikesharp/configs/DiscordChat/DiscordChat.json` using the `DiscordChatStyle` variable.

Pretty Style (`"DiscordChatStyle": 1`):

![Pretty Style](https://i.rebooti.ng/f/UrTSgA9d5b.png)

Simple Style (`"DiscordChatStyle": 0`):

![Simple Style](https://i.rebooti.ng/f/gzYZUsOOVO.png)

If you are looking to use this plugin purely to log chats, I recommend using the simple style. While it may not be as pretty as the other option, it makes searching SteamIDs in Discord possible. 

## How to Install:
You need CounterStrikeSharp installed and running on your server.

- Download the `CS2DiscordChat.zip` package from the latest release: https://github.com/1zc/CS2-Discord-Chat/releases/latest
- Extract the ZIP file to your game-directory folder (`game/csgo/`).

## How to Configure:

All configuration is done in `game/csgo/addons/counterstrikesharp/configs/DiscordChat/DiscordChat.json`. 

### Setting up `DiscordChatWebhook`:
The plugin needs a WebHook URL from Discord to be able to send chat messages to. Follow the steps below if you are unsure how this can be done:

* ***Step 1:*** Edit a channel > enter the Webhooks section inside the Integrations sub-menu > Make a new webhook.
* ***Step 2:*** Customize your new webhook! I recommend naming it according to the server you're going to use the webhook for, and adding an avatar related to your servers. (Making separate webhooks, accordingly named, for each server you host is a great way to identify what server a chat message was sent in!)
* ***Step 3:*** Copy your webhook URL, go back to `DiscordChat.json`, and configure `DiscordChatWebhook` to your webhook URL.

![Webhook Setup](https://infra.s-ul.eu/PGIRZY4W)

### Setting up `DiscordChatSteamKey`:
The plugin uses a SteamAPI key to access the Steam Web API to get player's profile pictures. This is an optional ConVar, disabling it will default the plugin to the simple webhook style since it can't pull profile pictures.

You can get your SteamAPI key here: https://steamcommunity.com/dev/apikey (**DO NOT SHARE THIS KEY WITH ANYONE.**)
