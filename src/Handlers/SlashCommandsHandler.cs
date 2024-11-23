using Discord.Interactions;
using Discord.WebSocket;
using WatchDuck.Helpers;

namespace WatchDuck.Handlers;


public class SlashCommandsHandler
{
    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;


    public SlashCommandsHandler(DiscordSocketClient discordClient, InteractionService interactions, IServiceProvider services)
    {
        _discordClient = discordClient;
        _interactions = interactions;
        _services = services;
    }


    public Task HandleSlashCommand(SocketSlashCommand command)
    {
        Task.Run(async () =>
        {
            try
            {
                if (command.User.Id != BotConfig.ADMIN_USER_ID)
                {
                    return;
                }

                var context = new InteractionContext(_discordClient, command, command.Channel);
                await _interactions.ExecuteCommandAsync(context, _services);
            }
            catch (Exception e)
            {
                await _discordClient.ReportErrorAsync(e);
            }
        });

        return Task.CompletedTask;
    }
}
