using System.Collections.Concurrent;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using WatchDuck.Handlers;
using WatchDuck.Helpers;
using WatchDuck.Models;

namespace WatchDuck;


public class WatchDuckBot
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();


    public static readonly ConcurrentDictionary<ulong, WatchedUser> WatchedUsers = new();


    public static async Task RunAsync()
    {
        var watchDuck = new WatchDuckBot();

        await watchDuck._discordClient.LoginAsync(TokenType.Bot, BotConfig.BOT_TOKEN);
        await watchDuck._discordClient.StartAsync();

        if (BotConfig.PLAYING_STATUS.Length != 0)
        {
            await watchDuck._discordClient.SetGameAsync(BotConfig.PLAYING_STATUS);
            _log.Info($"[ Playing status - {BotConfig.PLAYING_STATUS} ]");
        }

        await Task.Delay(-1);
    }


    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactionService;
    private readonly ServiceProvider _services;
    private WatchDuckBot()
    {
        var config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent,
            ConnectionTimeout = 30_000,
            DefaultRetryMode = RetryMode.RetryRatelimit,
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)TimeSpan.FromMinutes(5).TotalSeconds,
        };

        _discordClient = new DiscordSocketClient(config);
        _interactionService = new InteractionService(_discordClient.Rest);

        _services = DependencyInjectionHelper.BuildServiceProvider(_discordClient, _interactionService);
        _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services).Wait();

        BindEvents();
    }


    private void BindEvents()
    {
        _discordClient.Log += OnLog;
        _discordClient.Ready += async () =>
        {
            var adminGuild = _discordClient.Guilds.First(g => g.Id == BotConfig.ADMIN_GUILD_ID);
            await RolesHelper.InitializeAsync(adminGuild);
            await _interactionService.RegisterCommandsToGuildAsync(adminGuild.Id);
        };

        _discordClient.MessageReceived += (socketMessage) =>
        {
            var messagesHandler = _services.GetRequiredService<MessagesHandler>();
            return messagesHandler.HandleMessage(socketMessage);
        };

        _discordClient.ReactionAdded += (message, _, socketReaction) =>
        {
            var reactionsHandler = _services.GetRequiredService<ReactionsHandler>();
            return reactionsHandler.HandleReactionAdded(message, socketReaction);
        };

        _discordClient.ReactionRemoved += (message, _, socketReaction) =>
        {
            var reactionsHandler = _services.GetRequiredService<ReactionsHandler>();
            return reactionsHandler.HandleReactionRemoved(message, socketReaction);
        };

        _discordClient.SlashCommandExecuted += (command) =>
        {
            var slashCommandsHandler = _services.GetRequiredService<SlashCommandsHandler>();
            return slashCommandsHandler.HandleSlashCommand(command);
        };
    }



    private static Task OnLog(LogMessage msg)
    {
        if (msg.Severity is LogSeverity.Error or LogSeverity.Critical)
        {
            _log.Error(msg.ToString());
        }
        else
        {
            _log.Info(msg.ToString());
        }

        return Task.CompletedTask;
    }
}
