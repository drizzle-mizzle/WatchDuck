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

            _client.Ready += () =>
            {
                Task.Run(async () => await CreateRolesAndSlashCommandsAsync());
                //Task.Run(async () => await RunJobs());

                return Task.CompletedTask;
            };

            _client.JoinedGuild += (guild) =>
            {
                Task.Run(async () => await OnGuildJoinAsync(guild));
                return Task.CompletedTask;
            };

            await _client.LoginAsync(TokenType.Bot, ConfigFile.DiscordBotToken.Value);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task RunJobs()
        {
            while (true)
            {
                await Task.Delay(3_600_000); // 1 hour
            }
        }

        private async Task OnGuildJoinAsync(SocketGuild guild)
        {
            try
            {
                await _interactions.RegisterCommandsToGuildAsync(guild.Id);
                if (!guild.Roles.Any(r => r.Name == DucklingsRole))
                    await guild.CreateRoleAsync(DucklingsRole);

                if (!guild.Roles.Any(r => r.Name == CharEngineSubscriberRole))
                    await guild.CreateRoleAsync(CharEngineSubscriberRole);
            }
            catch (Exception e)
            {
                LogException(new[] { e });
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

        private async Task CreateRolesAndSlashCommandsAsync()
        {
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            foreach (var guild in _client.Guilds)
            {
                try
                {
                    await _interactions.RegisterCommandsToGuildAsync(guild.Id);
                    if (!guild.Roles.Any(r => r.Name == DucklingsRole))
                        await guild.CreateRoleAsync(DucklingsRole);

                    if (!guild.Roles.Any(r => r.Name == CharEngineSubscriberRole))
                        await guild.CreateRoleAsync(CharEngineSubscriberRole);
                }
                catch { continue; }
            }
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
