using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DuckBot.Models.Common;
using System.Text.RegularExpressions;
using static DuckBot.Services.CommonService;

namespace DuckBot.Services
{
    public static partial class CommandsService
    {
        internal static string RemoveFirstMentionPrefx(this string text)
            => MentionRegex().Replace(text, "", 1).Trim();

        internal static bool IsServerOwner(this SocketGuildUser? user)
            => user is not null && user.Id == user.Guild.OwnerId;

        internal static Embed SuccessEmbed()
            => $"{OK_SIGN_DISCORD} Success".ToInlineEmbed(Color.Green);

        [GeneratedRegex("\\<(.*?)\\>")]
        private static partial Regex MentionRegex();
    }
}
