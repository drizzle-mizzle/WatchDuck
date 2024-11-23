using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using WatchDuck.Helpers;

namespace WatchDuck.Handlers.SlashCommands;


[RequireUserPermission(GuildPermission.Administrator)]
[Group("admin", "Admin commands")]
public class AdminCommands : InteractionModuleBase<InteractionContext>
{
    private readonly DiscordSocketClient _discordClient;


    public AdminCommands(DiscordSocketClient discordClient)
    {
        _discordClient = discordClient;

    }


    [SlashCommand("say", "Say")]
    public async Task Say(string text)
    {
        await RespondAsync("🦆", ephemeral: true);
        await Context.Channel.SendMessageAsync(text);
    }


    [SlashCommand("repeat", "Repeat")]
    public async Task Repeat(string messageId, string? channelId = null)
    {
        await RespondAsync("🦆", ephemeral: true);

        IMessage message;
        if (channelId is null)
        {
            message = await Context.Channel.GetMessageAsync(ulong.Parse(messageId));
        }
        else
        {
            var channel = (IMessageChannel)await Context.Guild.GetChannelAsync(ulong.Parse(channelId));
            message = await channel.GetMessageAsync(ulong.Parse(messageId));
        }

        await Context.Channel.SendMessageAsync(message.Content, embeds: message.Embeds.Count == 0 ? null : (Embed[])message.Embeds);
    }


    [SlashCommand("clear-webhook", "Clear Webhook")]
    public async Task ClearWebhooks()
    {
        await DeferAsync();

        var channel = (ITextChannel)Context.Channel;
        var webhooks = await channel.GetWebhooksAsync();

        var response = new StringBuilder();
        foreach (var webhook in webhooks)
        {
            await webhook.DeleteAsync();
            response.AppendLine($"- **{webhook.Name}** ({webhook.Id})");
        }

        var embed = new EmbedBuilder().WithColor(Color.Green).WithTitle($"Deleted webhooks ({webhooks.Count})").WithDescription(response.ToString());
        await FollowupAsync("🦆", embed: embed.Build());
    }


    [SlashCommand("shutdown", "Shutdown")]
    public async Task AdminShutdownAsync()
    {
        await RespondAsync(embed: "D:".ToInlineEmbed(Color.Orange));
        Environment.Exit(0);
    }


    [SlashCommand("ping", "ping")]
    public async Task Ping()
        => await RespondAsync(embed: $":ping_pong: {_discordClient.Latency}ms".ToInlineEmbed(Color.Red));

}
