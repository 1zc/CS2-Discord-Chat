/*
                      ___  _____  _________  ___ 
                     ___  /  _/ |/ / __/ _ \/ _ |
                    ___  _/ //    / _// , _/ __ |
                   ___  /___/_/|_/_/ /_/|_/_/ |_|
    
    Plugin to relay text chat on a Counter-Strike: 2 server to a webhook.
    Copyright (C) 2024  Liam C. (Infra)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    Source: https://github.com/1zc/CS2-Discord-Chat 
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using RestSharp;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using System.ComponentModel;
using System.Diagnostics;

namespace DiscordChat;

public class DiscordChatConfig : BasePluginConfig
{
    [JsonPropertyName("DiscordChatWebhook")] public string DiscordChatWebhook { get; set; } = "";
    [JsonPropertyName("DiscordChatStyle")] public int DiscordChatStyle { get; set; }
    [JsonPropertyName("DiscordChatSteamKey")] public string DiscordChatSteamKey { get; set; } = "";
}

[MinimumApiVersion(160)]
public partial class DiscordChat : BasePlugin, IPluginConfig<DiscordChatConfig>
{
    // Metadata
    public override string ModuleName => "CS2 Discord Chat";
    public override string ModuleVersion => "1.0";
    public override string ModuleDescription => "Plugin to relay in-game text chat to a webhook. https://github.com/1zc/CS2-Discord-Chat";
    public override string ModuleAuthor => "Liam C. (Infra)";

    // Configuraton
    public DiscordChatConfig Config { get; set; } = new DiscordChatConfig();
    public void OnConfigParsed(DiscordChatConfig config)
    {
        // Validate Config
        if (config.DiscordChatWebhook == null)
        {
            throw new InvalidOperationException("[DISCORDCHAT ERROR] Failed to load configuration >> DiscordWebhook is a required field in the configuration file.");
        }

        // Load config
        Config = config;
    }

    // Global
    public Dictionary<ulong, string> steamAvatars = new Dictionary<ulong, string>();

    public class DiscordMessage
    {
        /// <summary>
        /// Plain-text message to send in the webhook.
        /// </summary>
        /// <remarks>Default: ""</remarks>
        public string content { get; set; } = "";
        /// <summary>
        /// Custom username to use when sending the message webhook. Leave blank to disable.
        /// </summary>
        /// <remarks>Default: null</remarks>
        public string? username { get; set; } = null;
        /// <summary>
        /// Custom avatar to use when sending the message webhook. Leave blank to disable.
        /// </summary>
        /// <remarks>Default: null</remarks>
        public string? avatar_url { get; set; } = null;
        /// <summary>
        /// Boolean to trigger TTS (Text-to-Speech) on the message. 
        /// </summary>
        /// <remarks>Default: false</remarks>
        public bool tts { get; set; } = false;
    }

    // Chat handler
    public HookResult OnCommandSay(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || !player.IsValid || player.IsBot || player.IsHLTV || info.GetArg(1).Length == 0) 
            return HookResult.Continue;

        // Async send message
        Task sendToDiscord = SendToDiscord(info.GetArg(1), player);
		return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        // Async get avatar
        Task getSteamAvatar = GetSteamAvatar(@event.Userid.SteamID);
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        try {
            steamAvatars.Remove(@event.Userid.SteamID);
        } catch {
            // ignore
        }

        return HookResult.Continue;
    }

    public async Task SendToDiscord(string text, CCSPlayerController player)
    {
        // Craft Webhook
        // To-do: Implement styling
        text = SanitiseMessage(text).Trim();
        if (text.Length == 0) 
            return;

        DiscordMessage message = new DiscordMessage{content = $"{player.PlayerName!} (`{player.SteamID!}`): `"+text+"`"};

        // string avatar = steamAvatars[player.SteamID];
        // Console.WriteLine("DEBUG >> " + Config.DiscordChatStyle);
        Server.NextFrame(() => player.PrintToChat($"CAN U SEE THIS {steamAvatars.ContainsKey(player.SteamID)}"));
        if (Config.DiscordChatStyle == 1)
        {
            // Console.WriteLine("DEBUG >> " + steamAvatars[player.SteamID]);
            // To-do: Implement modern styling
            message = new DiscordMessage
            {
                content = "`"+text+"`",
                username = player.PlayerName+" ("+player.SteamID+")",
                avatar_url = steamAvatars.ContainsKey(player.SteamID) ? steamAvatars[player.SteamID] : null,
            };
        }

        // Craft request
        RestClient DiscordClient = new RestClient(Config.DiscordChatWebhook);
        RestRequest DiscordRequest = new RestRequest("", Method.Post);
        DiscordRequest.AddHeader("Content-Type", "application/json");
        DiscordRequest.AddJsonBody(message);

        // Send request
        await DiscordClient.PostAsync(DiscordRequest);
    }

    public override void Load(bool hotReload)
    {
        // Hook chat listeners
        AddCommandListener("say", OnCommandSay);
        AddCommandListener("say_team", OnCommandSay);
    }

    private string SanitiseMessage(string message)
    {
        message = message.Replace("@", "");
        message = message.Replace(@"`", "");
        message = message.Replace(@"\", "");

        return message;
    }

    public class SteamResponse
    {
        public SteamAPIPlayer[]? players { get; set; }
    }

    public class SteamAPIPlayer
    {
        public string? steamid { get; set; }
        public int? communityvisibilitystate { get; set; }
        public int? profilestate { get; set; }
        public string? personaname { get; set; }
        public int? commentpermission { get; set; }
        public string? profileurl { get; set; }
        public string? avatar { get; set; }
        public string? avatarmedium { get; set; }
        public string? avatarfull { get; set; }
        public string? personastate { get; set; }
        public string? lastlogoff { get; set; }
        public string? realname { get; set; }
        public string? primaryclanid { get; set; }
        public string? timecreated { get; set; }
        public string? personastateflags { get; set; }
        public string? loccountrycode { get; set; }
        public string? locstatecode { get; set; }
        public string? loccityid { get; set; }
    }

    private async Task GetSteamAvatar(ulong steamID)
    {
        // To-do: Implement Steam API
        RestClient SteamClient = new RestClient("https://api.steampowered.com");
        RestRequest SteamRequest = new RestRequest($"/ISteamUser/GetPlayerSummaries/v2/?key={Config.DiscordChatSteamKey}&steamids={steamID}&format=json", Method.Get);
    
        // Send request
        RestResponse SteamResponseObj = await SteamClient.GetAsync(SteamRequest);
        SteamResponse response = JsonSerializer.Deserialize<SteamResponse>(SteamResponseObj.Content!)!;

        // Parse response
        if (response != null)
        {
            // if (steamAvatars.ContainsKey(steamID)) 
            //     return;
            
            steamAvatars.Add(steamID, response.players![0].avatarfull!);
        }

        else
        {
            throw new Exception("DEBUG >> Failed to grab Steam API object");
        }
    }
}
