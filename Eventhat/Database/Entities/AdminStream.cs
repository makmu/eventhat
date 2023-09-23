namespace Eventhat.Database.Entities;

public class AdminStream
{
    public AdminStream(string streamName, int messageCount, Guid lastMessageId, int lastMessageGlobalPosition)
    {
        StreamName = streamName;
        MessageCount = messageCount;
        LastMessageId = lastMessageId;
        LastMessageGlobalPosition = lastMessageGlobalPosition;
    }

    public string StreamName { get; }
    public int MessageCount { get; }
    public Guid LastMessageId { get; }
    public int LastMessageGlobalPosition { get; }
}