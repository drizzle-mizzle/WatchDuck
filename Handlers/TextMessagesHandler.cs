using Discord;
using Discord.Webhook;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using static DuckBot.Services.CommonService;


namespace DuckBot.Handlers
{
    internal partial class TextMessagesHandler
    {
        private readonly Dictionary<ulong, ulong> Users = new();
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
            if (userMessage.Author is not SocketGuildUser user) return;

            if (!Users.ContainsKey(user.Id))
            {
                ulong amount = 0;
                foreach (var channel in context.Guild.Channels)
                {
                    try
                    {
                        if (channel is SocketTextChannel stc)
                            amount += (ulong)((await stc.GetMessagesAsync(1000).FlattenAsync())?.Where(m => m.Author.Id == user.Id)?.Count() ?? 0);
                    } catch { continue; };
                }
                    
                Users.Add(user.Id, amount);
            }

            Users[user.Id]++;

            ulong totalAmountOfMessages = Users[user.Id];
            SocketRole? role;
            switch (totalAmountOfMessages)
            {
                case 1:
                    if (user.Roles.Any(r => r.Name == ROLE_HATCHLING || r.Name == ROLE_NESTLING || r.Name == ROLE_FLEDGLING || r.Name == ROLE_GROWNUP)) break;

                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_HATCHLING);
                    if (role is not null) await user.AddRoleAsync(role);

                    break;
                case 5:
                    if (user.Roles.Any(r => r.Name == ROLE_NESTLING || r.Name == ROLE_FLEDGLING || r.Name == ROLE_GROWNUP)) break;

                    var hatRole = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_HATCHLING);
                    if (hatRole is not null) await user.RemoveRoleAsync(hatRole);

                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_NESTLING);
                    if (role is not null) await user.AddRoleAsync(role);

                    break;
                case 50:
                    if (user.Roles.Any(r => r.Name == ROLE_FLEDGLING || r.Name == ROLE_GROWNUP)) break;

                    var nestRole = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_NESTLING);
                    if (nestRole is not null) await user.RemoveRoleAsync(nestRole);

                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_FLEDGLING);
                    if (role is not null) await user.AddRoleAsync(role);

                    break;
                case 100:
                    if (user.Roles.Any(r => r.Name == ROLE_GROWNUP)) break;

                    var fledRole = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_FLEDGLING);
                    if (fledRole is not null) await user.RemoveRoleAsync(fledRole);

                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_GROWNUP);
                    if (role is not null) await user.AddRoleAsync(role);

                    break;
            }

            bool userIsBadDuckling = await ValidateUser(context);
            if (userIsBadDuckling)
            {
                await user.BanAsync();

                foreach (var channel in context.Guild.Channels)
                    foreach(var message in await textChannel.GetMessagesAsync().FlattenAsync())
                        if (Equals(message.Author.Id, user.Id))
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
                await context.Channel.SendMessageAsync(embed: $"{context.User.Mention} was a very, very bad duckling and *accidentally* has drown in the lake.".ToInlineEmbed(Color.Magenta));
                return true;
            }

            return false;
        }

        [GeneratedRegex("\\<(.*?)\\>")]
        private static partial Regex MentionRegex();
    }
}
