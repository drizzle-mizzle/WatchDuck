using Discord;

namespace DuckBot.Services
{
    internal static partial class CommonService
    {
        internal static readonly string WARN_SIGN_UNICODE = "⚠";
        internal static readonly string WARN_SIGN_DISCORD = ":warning:";
        internal static readonly string OK_SIGN_DISCORD = ":white_check_mark: ";

        internal static Emoji DUCK_EMOJI = new("\uD83E\uDD86");
        internal static string DucklingsRole = "ducklings";

        internal static Emoji RADIO_EMOJI = new("\uD83D\uDCFB");
        internal static string CharEngineSubscriberRole = "CharacterEngine Subscriber";

        internal static string WAIT_MESSAGE = $"🕓 Wait...";
    }
}
