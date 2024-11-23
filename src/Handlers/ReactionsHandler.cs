using Discord;
using Discord.WebSocket;
using WatchDuck.Helpers;

namespace WatchDuck.Handlers;


internal class ReactionsHandler
{
    private readonly DiscordSocketClient _discordClient;


    public ReactionsHandler(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;
    }


    public Task HandleReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, SocketReaction reaction)
    {
        Task.Run(async () =>
        {
            try
            {
                var userId = reaction.User.GetValueOrDefault()?.Id;
                if (userId is null)
                {
                    return;
                }

                var guild = _discordClient.Guilds.First(g => g.Id == BotConfig.ADMIN_GUILD_ID);
                var user = guild.GetUser((ulong)userId);
                if (user is not IGuildUser guildUser || guildUser.IsDuckling() || guildUser.IsBadDuckling())
                {
                    return;
                }

                var message = await cachedMessage.GetOrDownloadAsync();
                if (message.Author.Id != _discordClient.CurrentUser.Id)
                {
                    return;
                }

                if (reaction.Emote.Name != "\ud83e\udd86") // 🦆
                {
                    return;
                }

                await guildUser.AddRoleAsync(RolesHelper.DUCKLINGS_ROLE.Id);
            }
            catch (Exception e)
            {
                await _discordClient.ReportErrorAsync(e);
            }
        });

        return Task.CompletedTask;
    }


    public Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, SocketReaction reaction)
    {
        Task.Run(async () =>
        {
            try
            {
                var userId = reaction.User.GetValueOrDefault()?.Id;
                if (userId is null)
                {
                    return;
                }

                var guild = _discordClient.Guilds.First(g => g.Id == BotConfig.ADMIN_GUILD_ID);
                var user = guild.GetUser((ulong)userId);
                if (user is not IGuildUser guildUser || guildUser.IsDuckling() == false)
                {
                    return;
                }

                var message = await cachedMessage.GetOrDownloadAsync();
                if (message is null || message.Author.Id != _discordClient.CurrentUser.Id)
                {
                    return;
                }

                if (reaction.Emote.Name != "\ud83e\udd86") // 🦆
                {
                    return;
                }

                await guildUser.RemoveRoleAsync(RolesHelper.DUCKLINGS_ROLE.Id);
            }
            catch (Exception e)
            {
                await _discordClient.ReportErrorAsync(e);
            }
        });

        return Task.CompletedTask;
    }


}
