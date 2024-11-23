using Discord;

namespace WatchDuck.Exceptions;


public class UserFriendlyException : Exception
{
    public bool Bold { get; }


    public Embed ToEmbed => new EmbedBuilder().Build();

    public UserFriendlyException(string message, bool bold = true) : base(message)
    {
        Bold = bold;
    }
}
