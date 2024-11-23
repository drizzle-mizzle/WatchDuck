using Discord;
using NLog;

namespace WatchDuck.Helpers;


public static class MessagesHelper
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();


    private const int MSG_LIMIT = 1990;
    public static async Task ReportErrorAsync(this IDiscordClient discordClient, Exception e)
    {
        var content = $"Exception:\n{e}";
        _log.Error(content);

        try
        {
            var channel = (ITextChannel)await discordClient.GetChannelAsync(BotConfig.LOGS_CHANNEL_ID);
            var thread = await channel.CreateThreadAsync("QUACK!?", autoArchiveDuration: ThreadArchiveDuration.ThreeDays);

            var count = 0;
            while (content.Length > 0)
            {
                if (count == 5)
                {
                    break;
                }

                count++;
                if (content.Length <= MSG_LIMIT)
                {
                    await thread.SendMessageAsync($"```js\n{content}```");
                    break;
                }

                await thread.SendMessageAsync(text: $"```js\n{content[..(MSG_LIMIT-1)]}```");
                content = content[MSG_LIMIT..];
            }
        }
        catch (Exception ex)
        {
            _log.Error($"[ FAILIED TO REPORT ERROR IN DISCORD! ]\n{ex}");
        }
    }


    public static async Task ReportLogAsync(this IDiscordClient discordClient, string title, string? content = null, string? imageUrl = null, Color? color = null, bool logToConsole = false)
    {
        if (logToConsole)
        {
            _log.Info(content is null ? title : $"[ {title} ] {content}");
        }

        try
        {
            var channel = (ITextChannel)await discordClient.GetChannelAsync(BotConfig.LOGS_CHANNEL_ID);
            if (content is null)
            {
                await channel.SendMessageAsync(embed: title.ToInlineEmbed(color ?? Color.Green, false, imageUrl, imageAsThumb: true));
                return;
            }

            if (content.Length < 1000)
            {
                await channel.SendMessageAsync(embeds: [title.ToInlineEmbed(color ?? Color.Green, false), content.ToInlineEmbed(Color.LightGrey, false, imageUrl, imageAsThumb: true)]);
                return;
            }

            var message = await channel.SendMessageAsync(embed: title.ToInlineEmbed(color ?? Color.Green, false, imageUrl, imageAsThumb: true));
            var thread = await channel.CreateThreadAsync("[ Details ]", autoArchiveDuration: ThreadArchiveDuration.ThreeDays, message: message);
            while (content.Length > 0)
            {
                if (content.Length <= MSG_LIMIT)
                {
                    await thread.SendMessageAsync(content);
                    break;
                }

                await thread.SendMessageAsync(content[..(MSG_LIMIT - 1)]);
                content = content[MSG_LIMIT..];
            }
        }
        catch (Exception e)
        {
            _log.Error($"[ FAILIED TO REPORT LOG IN DISCORD! ]\n{e}");
        }
    }


    public static Embed ToInlineEmbed(this string text, Color color, bool bold = true, string? imageUrl = null, bool imageAsThumb = false)
    {
        var desc = bold ? $"**{text}**" : text;

        var embed = new EmbedBuilder().WithDescription(desc).WithColor(color);

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return embed.Build();
        }

        if (imageAsThumb)
        {
            embed.WithThumbnailUrl(imageUrl);
        }
        else
        {
            embed.WithImageUrl(imageUrl);
        }

        return embed.Build();
    }
}
