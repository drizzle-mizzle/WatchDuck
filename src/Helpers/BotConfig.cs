using NLog;

namespace WatchDuck.Helpers;


public static class BotConfig
{
    public static string BOT_TOKEN
        => GetParamByName<string>("BOT_TOKEN").Trim();

    public static string PLAYING_STATUS
        => GetParamByName<string>("PLAYING_STATUS").Trim();

    public static ulong ADMIN_USER_ID
        => GetParamByName<ulong>("ADMIN_USER_ID");
    public static ulong ADMIN_GUILD_ID
        => GetParamByName<ulong>("ADMIN_GUILD_ID");

    public static ulong LOGS_CHANNEL_ID
        => GetParamByName<ulong>("LOGS_CHANNEL_ID");

    public static int USER_RATE_LIMIT
        => GetParamByName<int>("USER_RATE_LIMIT");

    public static int USER_FIRST_BLOCK_MINUTES
        => GetParamByName<int>("USER_FIRST_BLOCK_MINUTES");

    public static int USER_SECOND_BLOCK_HOURS
        => GetParamByName<int>("USER_SECOND_BLOCK_HOURS");


    private static string CONFIG_PATH = default!;
    public static void Initialize()
    {
        var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings"));
        CONFIG_PATH = files.GetFileThatStartsWith("env.config") ??
                      files.GetFileThatStartsWith("config")!;

        LogManager.GetCurrentClassLogger().Info($"[ Config path: {CONFIG_PATH} ]");
    }


    private static string? GetFileThatStartsWith(this string[] paths, string pattern)
        => paths.FirstOrDefault(file => file.Split(Path.DirectorySeparatorChar).Last().StartsWith(pattern));


    private static T GetParamByName<T>(string paramName) where T : notnull
    {
        var configLines = File.ReadAllLines(CONFIG_PATH);
        var neededLine = configLines.First(line => line.Trim().StartsWith(paramName)) + " ";
        var valueIndex = neededLine.IndexOf(':') + 1;
        var configValue = neededLine[valueIndex..].Trim();

        return string.IsNullOrWhiteSpace(configValue) ? default! : (T)Convert.ChangeType(configValue, typeof(T));
    }
}
