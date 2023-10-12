using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using DuckBot.Handlers;
using DuckBot.Models.Common;
using static DuckBot.Services.CommonService;

namespace DuckBot.Services
{
    internal class DiscordService
    {
        private ServiceProvider _services = null!;
        private DiscordSocketClient _client = null!;
        private InteractionService _interactions = null!;

        internal async Task SetupDiscordClient()
        {
            _services = CreateServices();

            _client = _services.GetRequiredService<DiscordSocketClient>();
            _interactions = _services.GetRequiredService<InteractionService>();

            // Initialize handlers
            _services.GetRequiredService<ReactionsHandler>();
            _services.GetRequiredService<SlashCommandsHandler>();
            _services.GetRequiredService<TextMessagesHandler>();

            _client.Ready += async () =>
            {
                LogGreen(new string('~', Console.WindowWidth));
                Log($"Guilds: {_client.Guilds.Count}\n");
                try { await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services); }
                catch (Exception e) { LogException(new[] { e }); }

                foreach (var guild in _client.Guilds)
                    await CreateRolesAndSlashCommandsAsync(guild);               
            };
            _client.JoinedGuild += CreateRolesAndSlashCommandsAsync;

            await _client.LoginAsync(TokenType.Bot, ConfigFile.DiscordBotToken.Value);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task CreateRolesAndSlashCommandsAsync(SocketGuild guild)
        {
            LogYellow($"Registerting commands to: {guild.Name}... ");

            try { await _interactions.RegisterCommandsToGuildAsync(guild.Id); }
            catch (Exception e) { LogRed($"Fail!\n{e}\n"); return; }

            LogGreen("OK\n");

            foreach (string role in ALL_ROLES)
            {
                if (guild.Roles.Any(r => r.Name == role)) continue;
                else await guild.CreateRoleAsync(role);
            }
        }


        internal static ServiceProvider CreateServices()
        {
            var discordClient = CreateDiscordClient();
            var services = new ServiceCollection()
                .AddSingleton(discordClient)
                .AddSingleton<SlashCommandsHandler>()
                .AddSingleton<TextMessagesHandler>()
                .AddSingleton<ReactionsHandler>()
                .AddSingleton(new InteractionService(discordClient.Rest));

            return services.BuildServiceProvider();
        }

        private static DiscordSocketClient CreateDiscordClient()
        {
            // Define GatewayIntents
            var intents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.MessageContent | GatewayIntents.GuildWebhooks;

            // Create client
            var clientConfig = new DiscordSocketConfig { MessageCacheSize = 5, GatewayIntents = intents };
            var client = new DiscordSocketClient(clientConfig);

            // Bind event handlers
            client.Log += (msg) => Task.Run(() => Log($"{msg}\n"));

            return client;
        }
    }
}
