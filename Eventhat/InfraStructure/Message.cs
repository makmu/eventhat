namespace Eventhat.InfraStructure;

public class Message<TData>
{
    public Message(Guid id, string streamName, Metadata metadata, TData data, int position, int globalPosition)
    {
        Id = id;
        StreamName = streamName;
        Metadata = metadata;
        Data = data;
        Position = position;
        GlobalPosition = globalPosition;
    }

    public Guid Id { get; }
    public string StreamName { get; }
    public Metadata Metadata { get; }
    public TData Data { get; }
    public int GlobalPosition { get; }
    public int Position { get; }
}

public class Metadata
{
    public Metadata(Guid traceId, Guid userId, string? originStreamName = null)
    {
        TraceId = traceId;
        UserId = userId;
        OriginStreamName = originStreamName;
    }

    public Guid TraceId { get; }
    public Guid UserId { get; }
    public string? OriginStreamName { get; }
}