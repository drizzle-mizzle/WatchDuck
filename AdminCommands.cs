using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using static DuckBot.Services.CommonService;
using static DuckBot.Services.CommandsService;

namespace DuckBot.SlashCommands
{
    [Group("admin", "Admin commands")]
    public class AdminCommands : InteractionModuleBase<InteractionContext>
    {
        private readonly DiscordSocketClient _client;

        public AdminCommands(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        [SlashCommand("spawn-roles-selector", "-")]
        public async Task SpawnRolesSelector(string title, string desc, string? footer = null)
        {
            var embed = new EmbedBuilder().WithTitle(title).WithDescription(desc);
            if (footer is not null) embed.WithFooter(footer);

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());
            await message.AddReactionsAsync(new List<Emoji>() { DUCK_EMOJI, RADIO_EMOJI });
        }

        [SlashCommand("spawn-duck-only", "-")]
        public async Task SpawnDuckOnly(string title, string desc, string? footer = null)
        {
            var embed = new EmbedBuilder().WithTitle(title).WithDescription(desc);
            if (footer is not null) embed.WithFooter(footer);

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());
            await message.AddReactionsAsync(new List<Emoji>() { DUCK_EMOJI });
        }

        [SlashCommand("spawn-sub-only", "-")]
        public async Task SpawnSubOnly(string title, string desc, string? footer = null)
        {
            var embed = new EmbedBuilder().WithTitle(title).WithDescription(desc);
            if (footer is not null) embed.WithFooter(footer);

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());
            await message.AddReactionsAsync(new List<Emoji>() { RADIO_EMOJI });
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
    }
}
