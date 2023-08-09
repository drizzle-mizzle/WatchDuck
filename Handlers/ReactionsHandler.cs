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

            _client.ReactionAdded += (msg, channel, reaction) =>
            {
                Task.Run(async () => await HandleReactionAddedAsync(msg, channel, reaction));
                return Task.CompletedTask;
            };

            _client.ReactionRemoved += (msg, channel, reaction) =>
            {
                Task.Run(async () => await HandleReactionRemovedAsync(msg, channel, reaction));
                return Task.CompletedTask;
            };
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> discordChannel, SocketReaction reaction)
        {
            if (reaction.User.Value is not SocketGuildUser user) return;
            if (user.IsBot) return;

            var message = await rawMessage.DownloadAsync();
            if (message.Author.Id != _client.CurrentUser.Id) return;
            if (message.Channel is not SocketGuildChannel channel) return;

            if (reaction.Emote?.Name == DUCK_EMOJI.Name)
            {
                await user.AddRoleAsync(channel.Guild.Roles.FirstOrDefault(r => r.Name == DucklingsRole));
            }
            else if (reaction.Emote?.Name == RADIO_EMOJI.Name)
            {
                await user.AddRoleAsync(channel.Guild.Roles.FirstOrDefault(r => r.Name == CharEngineSubscriberRole));
            }
        }

        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> rawMessage, Cacheable<IMessageChannel, ulong> discordChannel, SocketReaction reaction)
        {
            if (reaction.User.Value is not SocketGuildUser user) return;
            if (user.IsBot) return;

            var message = await rawMessage.DownloadAsync();
            if (message.Author.Id != _client.CurrentUser.Id) return;
            if (message.Channel is not SocketGuildChannel channel) return;

            if (reaction.Emote?.Name == DUCK_EMOJI.Name)
            {
                await user.RemoveRoleAsync(channel.Guild.Roles.FirstOrDefault(r => r.Name == DucklingsRole));
            }
            else if (reaction.Emote?.Name == RADIO_EMOJI.Name)
            {
                await user.RemoveRoleAsync(channel.Guild.Roles.FirstOrDefault(r => r.Name == CharEngineSubscriberRole));
            }
        }

    }
}
