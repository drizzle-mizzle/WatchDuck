using Discord;
using Discord.Webhook;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using DuckBot.Models.Common;
using static DuckBot.Services.CommonService;
using System.Threading.Channels;

namespace DuckBot.Handlers
{
    internal partial class TextMessagesHandler
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;

        /// <summary>
        /// (User ID : [message content : repeat count])
        /// </summary>
        private readonly Dictionary<ulong, KeyValuePair<string, int>> _watchDog = new();

        public TextMessagesHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();

            _client.MessageReceived += (message) =>
            {
                Task.Run(async () => await HandleMessageAsync(message));
                return Task.CompletedTask;
            };
        }

        private async Task HandleMessageAsync(SocketMessage sm)
        {
            if (sm is not SocketUserMessage userMessage) return;
            if (userMessage.Author.IsBot || userMessage.Author.IsWebhook) return;
            if (userMessage.Author.Id == _client.CurrentUser.Id) return;

            var context = new SocketCommandContext(_client, userMessage);
            if (context.Guild is null) return;
            if (context.Channel is not SocketTextChannel textChannel) return;
            if (userMessage.Author.Id == context.Guild.OwnerId) return;

            bool userIsBadDuckling = await ValidateUser(context);
            if (userIsBadDuckling)
            {
                if (userMessage.Author is SocketGuildUser user)
                    await user.BanAsync();

                foreach (var channel in context.Guild.Channels)
                    foreach(var message in await textChannel.GetMessagesAsync().FlattenAsync())
                        if (Equals(message.Author.Id, context.Message.Author.Id))
                            await message.DeleteAsync();
            }
        }

        internal async Task<bool> ValidateUser(SocketCommandContext context)
        {
            ulong currUserId = context.Message.Author.Id;

            // Start watching for user
            if (!_watchDog.ContainsKey(currUserId))
                _watchDog.Add(currUserId, new(context.Message.Content ?? "", 0));

            if (!string.Equals(_watchDog[currUserId].Key, context.Message.Content))
            {
                _watchDog[currUserId] = new(context.Message.Content ?? "", 0);
                return false;
            }

            _watchDog[currUserId] = new(_watchDog[currUserId].Key, _watchDog[currUserId].Value + 1);

            if (_watchDog[currUserId].Value == 3)
            {
                await context.Message.ReplyAsync(embed: $"{context.User.Mention} Sssh...".ToInlineEmbed(Color.Orange));
                return false;
            }
            
            if (_watchDog[currUserId].Value == 5)
            {
                _watchDog.Remove(currUserId);
                await context.Channel.SendMessageAsync(embed: $"{context.User.Mention} was a very, very bad duckling and accidentally fell out of the nest".ToInlineEmbed(Color.Magenta));
                return true;
            }

            return false;
        }

        [GeneratedRegex("\\<(.*?)\\>")]
        private static partial Regex MentionRegex();
    }
}
