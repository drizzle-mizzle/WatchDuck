using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
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

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> rawChannel, SocketReaction reaction)
        {
            try
            {
                if (reaction.User.GetValueOrDefault() is not SocketGuildUser user) { LogRed("."); return; }
                if (user.IsBot) return;

                var message = await rawMessage.DownloadAsync();
                if (message.Author.Id != _client.CurrentUser.Id) return;
                if (message.Channel is not SocketGuildChannel channel) return;

                string? emojiName = reaction.Emote?.Name;
                if (string.IsNullOrWhiteSpace(emojiName)) return;

                if (FREE_ROLES.ContainsKey(emojiName))
                {
                    await user.AddRoleAsync(channel.Guild.Roles.First(r => r.Name == FREE_ROLES[emojiName]));
                }
                else { LogYellow("."); return; }
            }
            catch (Exception e)
            {
                LogException(new[] { e });
            }
        }

        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> rawChannel, SocketReaction reaction)
        {
            try
            {
                if (reaction.User.GetValueOrDefault() is not SocketGuildUser user) { LogRed("."); return; }
                if (user.IsBot) return;

                var message = await rawMessage.DownloadAsync();
                if (message.Author.Id != _client.CurrentUser.Id) return;
                if (message.Channel is not SocketGuildChannel channel) return;

                string? emojiName = reaction.Emote?.Name;
                if (string.IsNullOrWhiteSpace(emojiName)) return;

                if (FREE_ROLES.ContainsKey(emojiName))
                {
                    await user.RemoveRoleAsync(channel.Guild.Roles.First(r => r.Name == FREE_ROLES[emojiName]));
                }
                else { LogYellow("."); return; }
            }
            catch (Exception e)
            {
                LogException(new[] { e });
            }
        }

    }
}
