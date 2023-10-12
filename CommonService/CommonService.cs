using Discord;
using Newtonsoft.Json;
using System.Dynamic;

namespace DuckBot.Services
{
    internal static partial class CommonService
    {
        // Simply checks whether image is avalable.
        // (cAI is used to have broken undownloadable images or sometimes it's just
        //  takes eternity for it to upload one on server, but image url is provided in advance)
        public static async Task<bool> TryGetImageAsync(string url, HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            for (int i = 0; i < 10; i++)
                if ((await httpClient.GetAsync(url).ConfigureAwait(false)).IsSuccessStatusCode)
                    return true;
                else
                    await Task.Delay(3000);

            return false;
        }

        public static async Task<Stream?> TryDownloadImgAsync(string? url, HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            for (int i = 0; i < 10; i++)
            {
                try {
                    var response = await httpClient.GetAsync(url).ConfigureAwait(false);
                    return await response.Content.ReadAsStreamAsync();
                }
                catch { await Task.Delay(3000); }
            }

            return null;
        }

        internal static Embed ToInlineEmbed(this string text, Color color, bool bold = true)
        {
            string desc = bold ? $"**{text}**" : text;

            return new EmbedBuilder().WithDescription(desc).WithColor(color).Build();
        }

        public static bool ToBool(this string? str)
            => bool.Parse(str ?? "false");

        public static ulong ToUlong(this string? str)
            => str is null ? 0 : ulong.Parse(str);

        public static int ToInt(this string str)
            => int.Parse(str);

        public static bool IsEmpty(this string? str)
            => string.IsNullOrWhiteSpace(str);

        public static string RemovePrefix(this string str, string prefix)
        {
            var text = str.Trim();
            if (text.StartsWith(prefix))
                text = text.Remove(0, prefix.Length);

            return text;
        }

    }
}
