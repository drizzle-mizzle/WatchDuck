namespace WatchDuck.Models;


public class WatchedUser
{
    public List<UserMessage> CurrentWindowMessages { get; } = [];

    public required DateTime InteractionsWindowStartDt { get; set; }

    public required int TotalMessages { get; set; }

    public List<ulong> SeenInChannels { get; } = [];
}


public class UserMessage
{
    public required string Content { get; set; }
    public required long AttachmentsSumSize { get; set; }
}
