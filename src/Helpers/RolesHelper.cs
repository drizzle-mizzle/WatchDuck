using Discord;
using Discord.WebSocket;
using NLog;

namespace WatchDuck.Helpers;


public static class RolesHelper
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    public static SocketRole DUCKLINGS_ROLE = null!;
    public static SocketRole BAD_DUCKLING_ROLE = null!;

    public static SocketRole HATCHLING_ROLE = null!;
    public static SocketRole NESTLING_ROLE = null!;
    public static SocketRole FLEDGLING_ROLE = null!;
    public static SocketRole GROWN_UP_DUCKLING_ROLE = null!;


    public static async Task InitializeAsync(SocketGuild adminGuild)
    {
        DUCKLINGS_ROLE = await adminGuild.GetOrCreateRoleAsync("ducklings");
        BAD_DUCKLING_ROLE = await adminGuild.GetOrCreateRoleAsync("bad duckling");

        HATCHLING_ROLE = await adminGuild.GetOrCreateRoleAsync("hatchling");
        NESTLING_ROLE = await adminGuild.GetOrCreateRoleAsync("nestling");
        FLEDGLING_ROLE = await adminGuild.GetOrCreateRoleAsync("fledgling");
        GROWN_UP_DUCKLING_ROLE = await adminGuild.GetOrCreateRoleAsync("grown-up duckling");
    }


    public static async Task UpdateDucklingLevelAsync(IGuildUser guildUser)
    {
        if (guildUser.IsGrownUpDuckling())
        {
            return;
        }

        var userTotalMessages = WatchDuckBot.WatchedUsers[guildUser.Id].TotalMessages;
        if (guildUser.IsFledgling())
        {
            if (userTotalMessages > 100)
            {
                await guildUser.RemoveRoleAsync(FLEDGLING_ROLE.Id);
                await guildUser.AddRoleAsync(GROWN_UP_DUCKLING_ROLE.Id);
            }

            return;
        }

        if (guildUser.IsNestling())
        {
            if (userTotalMessages > 20)
            {
                await guildUser.RemoveRoleAsync(NESTLING_ROLE.Id);
                await guildUser.AddRoleAsync(FLEDGLING_ROLE.Id);
            }

            return;

        }

        if (guildUser.IsHatchling())
        {
            if (userTotalMessages > 4)
            {
                await guildUser.RemoveRoleAsync(HATCHLING_ROLE.Id);
                await guildUser.AddRoleAsync(NESTLING_ROLE.Id);
            }

            return;
        }

        await guildUser.AddRoleAsync(HATCHLING_ROLE.Id);
    }


    private static async Task<SocketRole> GetOrCreateRoleAsync(this SocketGuild guild, string roleName)
    {
        var role = guild.Roles.FirstOrDefault(r => r.Name == roleName);
        if (role is not null)
        {
            _log.Info($"[ Found role: {roleName} ]");
            return role;
        }

        _log.Info($"[ Creating new role: {roleName} ]");

        await guild.CreateRoleAsync(roleName);
        return await guild.GetOrCreateRoleAsync(roleName);
    }


    public static bool IsDuckling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == DUCKLINGS_ROLE.Id);

    public static bool IsBadDuckling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == BAD_DUCKLING_ROLE.Id);

    private static bool IsHatchling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == HATCHLING_ROLE.Id);

    private static bool IsNestling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == NESTLING_ROLE.Id);

    private static bool IsFledgling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == FLEDGLING_ROLE.Id);

    private static bool IsGrownUpDuckling(this IGuildUser guildUser)
        => guildUser.RoleIds.Any(roleId => roleId == GROWN_UP_DUCKLING_ROLE.Id);


}
