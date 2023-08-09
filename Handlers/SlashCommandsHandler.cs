using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;

namespace DuckBot.Handlers
{
    public class SlashCommandsHandler
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;

        public SlashCommandsHandler(IServiceProvider services)
        {
            _services = services;
            _interactions = _services.GetRequiredService<InteractionService>();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _client.SlashCommandExecuted += (command) =>
            {
                Task.Run(async () => await HandleCommandAsync(command));
                return Task.CompletedTask;
            };
        }

        private async Task HandleCommandAsync(SocketSlashCommand command)
        {
            var context = new InteractionContext(_client, command, command.Channel);
            await _interactions.ExecuteCommandAsync(context, _services);
        }
    }
}
