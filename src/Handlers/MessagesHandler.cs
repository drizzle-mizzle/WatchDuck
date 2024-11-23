using Discord;
using Discord.WebSocket;
using WatchDuck.Helpers;
using WatchDuck.Models;

namespace WatchDuck.Handlers;


internal class MessagesHandler
{
    private readonly DiscordSocketClient _discordClient;


    public MessagesHandler(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;
    }


    public Task HandleMessage(SocketMessage socketMessage)
    {
        Task.Run(async () =>
        {
            try
            {
                if (socketMessage.Author is not SocketGuildUser guildUser || guildUser.Id == BotConfig.ADMIN_USER_ID || guildUser.IsWebhook || guildUser.IsBot)
                {
                    return;
                }

                var validationResult = AnalyzeUserBehavior(socketMessage);
                switch (validationResult)
                {
                    case AnalysisResult.Warn:
                    {
                        await socketMessage.Channel.SendMessageAsync(guildUser.Mention, embed: "Be quiet".ToInlineEmbed(Color.Orange));
                        break;
                    }
                    case AnalysisResult.Block:
                    {
                        await guildUser.RemoveRoleAsync(RolesHelper.DUCKLINGS_ROLE.Id);
                        await guildUser.AddRoleAsync(RolesHelper.BAD_DUCKLING_ROLE);
                        await _discordClient.ReportLogAsync("🦆 BAD DUCKLING", $"User **{guildUser.Username} ({guildUser.Id})** was blocked", imageUrl: guildUser.GetAvatarUrl(), color: Color.Red);
                        await ClearSpamAsync(guildUser);

                        return;
                    }
                }

                await RolesHelper.UpdateDucklingLevelAsync(guildUser);
            }
            catch (Exception e)
            {
                await _discordClient.ReportErrorAsync(e);
            }
        });

        return Task.CompletedTask;
    }


    private static int BLOCK_THRESHOLD = BotConfig.USER_RATE_LIMIT;
    private enum AnalysisResult { Pass, Warn, Block }

    /// <summary>
    /// Checks if user is a dick
    /// </summary>
    private static AnalysisResult AnalyzeUserBehavior(SocketMessage socketMessage)
    {
        var watchedUserMessage = socketMessage.ToWatchedUserMessage();

        if (!WatchDuckBot.WatchedUsers.TryGetValue(socketMessage.Author.Id, out var watchedUser))
        {
            var newWatchedUser = new WatchedUser
            {
                InteractionsWindowStartDt = DateTime.Now,
                TotalMessages = 1,
            };

            newWatchedUser.CurrentWindowMessages.Add(watchedUserMessage);
            newWatchedUser.SeenInChannels.Add(socketMessage.Channel.Id);
            WatchDuckBot.WatchedUsers.TryAdd(socketMessage.Author.Id, newWatchedUser);

            return AnalysisResult.Pass;
        }

        lock (watchedUser)
        {
            watchedUser.TotalMessages++;

            var interactionWindowExpired = (DateTime.Now - watchedUser.InteractionsWindowStartDt).TotalSeconds >= 30; 
            if (interactionWindowExpired)
            {
                watchedUser.InteractionsWindowStartDt = DateTime.Now;
                watchedUser.CurrentWindowMessages.Clear();
                watchedUser.CurrentWindowMessages.Add(watchedUserMessage);
                watchedUser.SeenInChannels.Clear();
                watchedUser.SeenInChannels.Add(socketMessage.Channel.Id);

                return AnalysisResult.Pass;
            }

            watchedUser.CurrentWindowMessages.Add(watchedUserMessage);

            if (!watchedUser.SeenInChannels.Contains(socketMessage.Channel.Id))
            {
                watchedUser.SeenInChannels.Add(socketMessage.Channel.Id);
            }

            if (watchedUser.CurrentWindowMessages.Count < 5)
            {
                return AnalysisResult.Pass;
            }

            if (watchedUser.CurrentWindowMessages.Count > BLOCK_THRESHOLD)
            {
                return AnalysisResult.Block;
            }

            if (watchedUser.CurrentWindowMessages.Count > BLOCK_THRESHOLD - 2)
            {
                return AnalysisResult.Warn;
            }

            var messagesContents = watchedUser.CurrentWindowMessages.Select(m => m.Content).Where(c => c.Length != 0).ToArray();

            if (messagesContents.Length > 4)
            {
                var messagesContentsDistinct = messagesContents.Distinct().ToArray();
                if (messagesContentsDistinct.Length == 1 || (messagesContents.Length > 9 && messagesContentsDistinct.Length == 2))
                {
                    return AnalysisResult.Block;
                }
            }

            var messagesAttachmentsSizes = watchedUser.CurrentWindowMessages.Select(m => m.AttachmentsSumSize).Where(a => a != 0).ToArray();
            if (messagesAttachmentsSizes.Length > 4)
            {
                var messagesAttachmentsSizesDistinct = messagesAttachmentsSizes.Distinct().ToArray();
                if (messagesAttachmentsSizesDistinct.Length == 1 || (messagesAttachmentsSizes.Length > 9 && messagesAttachmentsSizesDistinct.Length == 2))
                {
                    return AnalysisResult.Block;
                }
            }

            return AnalysisResult.Pass;
        }
    }


    private async Task ClearSpamAsync(IGuildUser guildUser)
    {
        var watchedUser = WatchDuckBot.WatchedUsers[guildUser.Id];
        var channels = _discordClient.GetGuild(guildUser.GuildId).TextChannels.Where(tc => watchedUser.SeenInChannels.Contains(tc.Id));
        var now = DateTime.UtcNow;

        var spamMessages = new List<IMessage>();
        foreach (var channel in channels)
        {
            var currChannelSpamMessages = new List<IMessage>();

            var messages = await channel.GetMessagesAsync(30).FlattenAsync();
            foreach (var message in messages.Where(m => m.Author.Id == guildUser.Id).Reverse())
            {
                if (now - message.Timestamp.UtcDateTime < TimeSpan.FromSeconds(60))
                {
                    currChannelSpamMessages.Add(message);
                }
            }

            if (currChannelSpamMessages.Count > 0)
            {
                spamMessages.AddRange(currChannelSpamMessages);
                await channel.DeleteMessagesAsync(currChannelSpamMessages.Select(m => m.Id));
                await channel.SendMessageAsync(embed: $"{guildUser.Mention} turned out to be a very bad duckling and accidentally ended up drowning in the lake...".ToInlineEmbed(Color.DarkRed));
            }
        }

        var lines = spamMessages.Select(sm => $"Channel: **{sm.Channel.Id}**\n" +
                                              $"Content: {sm.Content?.Trim('\n', ' ') ?? "none"}");

        await _discordClient.ReportLogAsync("Spam deleted", string.Join('\n', lines), color: Color.Magenta);
    }

}


internal static class MessagesHandlerExtensions
{
    public static UserMessage ToWatchedUserMessage(this SocketMessage socketMessage)
    {
        var attachments = socketMessage.Attachments;
        return new UserMessage
        {
            Content = socketMessage.Content?.Trim(' ', '\n', '\r') ?? "",
            AttachmentsSumSize = attachments.Count == 0 ? 0 : attachments.Select(a => a.Size).Sum()
        };
    }
}
