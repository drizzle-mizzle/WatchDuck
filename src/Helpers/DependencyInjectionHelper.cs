using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WatchDuck.Handlers;

namespace WatchDuck.Helpers;


public static class DependencyInjectionHelper
{
    public static ServiceProvider BuildServiceProvider(DiscordSocketClient discordClient, InteractionService interactionService)
    {
        var services = new ServiceCollection();

        // Singleton
        {
            services.AddSingleton(discordClient);
            services.AddSingleton(interactionService);
        }

        // Scoped
        {
            services.AddScoped<SlashCommandsHandler>();
            services.AddScoped<MessagesHandler>();
            services.AddScoped<ReactionsHandler>();
        }

        return services.BuildServiceProvider();;
    }
}
