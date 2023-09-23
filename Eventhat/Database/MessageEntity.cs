namespace Eventhat.Database;

public class MessageEntity
{
    public MessageEntity(
        Guid id,
        string streamName,
        string type,
        int position,
        int globalPosition,
        string data,
        string metadata,
        DateTimeOffset time)
    {
        Id = id;
        StreamName = streamName;
        Type = type;
        Position = position;
        GlobalPosition = globalPosition;
        Data = data;
        Metadata = metadata;
        Time = time;
    }

    public Guid Id { get; }
    public string StreamName { get; }
    public string Type { get; }
    public int Position { get; }
    public int GlobalPosition { get; }
    public string Data { get; }
    public string Metadata { get; }
    public DateTimeOffset Time { get; }
}