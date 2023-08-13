﻿using Discord;
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

            if (!Users.ContainsKey(user.Id)) Users.Add(user.Id, 0);
            Users[user.Id]++;

            ulong totalAmountOfMessages = Users[user.Id];
            SocketRole? role;
            switch (totalAmountOfMessages)
            {
                case 1:
                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_HATCHLING);
                    if (role is not null) await user.AddRoleAsync(role);
                    break;
                case 5:
                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_NESTLING);
                    if (role is not null) await user.AddRoleAsync(role);
                    break;
                case 50:
                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_FLEDGLING);
                    if (role is not null) await user.AddRoleAsync(role);
                    break;
                case 100:
                    role = textChannel.Guild.Roles.FirstOrDefault(r => r.Name == ROLE_GROWNUP);
                    if (role is not null) await user.AddRoleAsync(role);
                    break;
            }

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
                await context.Channel.SendMessageAsync(embed: $"{context.User.Mention} was a very, very bad duckling and *accidentally* has drown in the lake.".ToInlineEmbed(Color.Magenta));
                return true;
            }

            return false;
        }

        [GeneratedRegex("\\<(.*?)\\>")]
        private static partial Regex MentionRegex();
    }
}
