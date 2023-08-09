using DuckBot.Services;
using Newtonsoft.Json.Linq;

namespace DuckBot.Models.Common
{
    public static class ConfigFile
    {
        //public static ConfigField NoPermissionFile { get; } = new("no_permission_file");
        public static ConfigField DiscordBotToken { get; } = new("discord_bot_token");
        private static JObject ConfigParsed { get; } = CommonService.TryToParseConfigFile();

        public class ConfigField
        {
            public readonly string Label;
            public string? Value {
                get
                {
                    string? data = ConfigParsed[Label]?.Value<string?>();
                    return string.IsNullOrWhiteSpace(data) ? null : data;
                }
            }

            public ConfigField(string label)
            {
                Label = label;
            }
        }
    }
}
