using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static DuckBot.Services.CommonService;

namespace DuckBot.Handlers
{
    internal class ReactionsHandler
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;

        public ReactionsHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _client.ReactionAdded += HandleReactionAddedAsync;
            _client.ReactionRemoved += HandleReactionRemovedAsync;
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.User.GetValueOrDefault() is not SocketGuildUser user) return;
                if (user.IsBot) return;

                var message = await rawMessage.DownloadAsync();
                if (message.Author.Id != _client.CurrentUser.Id) return;
                if (message.Channel is not SocketGuildChannel channel) return;

                string? emojiName = reaction.Emote?.Name;
                if (string.IsNullOrWhiteSpace(emojiName)) return;

                if (FREE_EMOJIS.Contains(emojiName))
                {
                    await user.AddRoleAsync(channel.Guild.Roles.First(r => r.Name == emojiName));
                }
            }
            catch (Exception e)
            {
                LogException(new[] { e });
            }
        }

        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.User.GetValueOrDefault() is not SocketGuildUser user) return;
                if (user.IsBot) return;

                var message = await rawMessage.DownloadAsync();
                if (message.Author.Id != _client.CurrentUser.Id) return;
                if (message.Channel is not SocketGuildChannel channel) return;

                string? emojiName = reaction.Emote?.Name;
                if (string.IsNullOrWhiteSpace(emojiName)) return;

                if (FREE_EMOJIS.Contains(emojiName))
                {
                    await user.RemoveRoleAsync(channel.Guild.Roles.First(r => r.Name == emojiName));
                }
            }
            catch (Exception e)
            {
                LogException(new[] { e });
            }
        }

    }
}
