using DuckBot.Services;

namespace DuckBot
{
    internal class Program : DiscordService
    {
        static void Main()
                    => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            await SetupDiscordClient();
            await Task.Delay(-1);
        }
    }
}