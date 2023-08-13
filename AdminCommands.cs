using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using static DuckBot.Services.CommonService;
using static DuckBot.Services.CommandsService;

namespace DuckBot.SlashCommands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    [Group("admin", "Admin commands")]
    public class AdminCommands : InteractionModuleBase<InteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public AdminCommands(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        [SlashCommand("post", "-")]
        public async Task PostAsync(string title, string desc, string? imgUrl = null, string? footer = null)
        {
            await DeferAsync();
            var embed = new EmbedBuilder().WithTitle(title).WithDescription(desc.Replace("\\n", "\n")).WithColor(Color.Green);
            if (imgUrl is not null) embed.WithImageUrl(imgUrl);
            if (footer is not null) embed.WithFooter(footer);

            await Context.Channel.SendMessageAsync(embed: embed.Build());
            await FollowupAsync(text: ":duck:");
        }

        [SlashCommand("shutdown", "Shutdown")]
        public async Task AdminShutdownAsync()
        {
            await RespondAsync(embed: $"{WARN_SIGN_DISCORD} Shutting down...".ToInlineEmbed(Color.Orange));
            Environment.Exit(0);
        }

        [SlashCommand("set-game", "Set game status")]
        public async Task AdminUpdateGame(string? activity = null, string? streamUrl = null, ActivityType type = ActivityType.Playing)
        {
            await _client.SetGameAsync(activity, streamUrl, type);
            await RespondAsync(embed: SuccessEmbed(), ephemeral: true);
        }

        [SlashCommand("set-status", "Set status")]
        public async Task AdminUpdateStatus(UserStatus status)
        {
            await _client.SetStatusAsync(status);
            await RespondAsync(embed: SuccessEmbed(), ephemeral: true);
        }

        [SlashCommand("ping", "ping")]
        public async Task Ping()
            => await RespondAsync(embed: $":ping_pong: Pong! - {_client.Latency} ms".ToInlineEmbed(Color.Red));
        
    }
}
