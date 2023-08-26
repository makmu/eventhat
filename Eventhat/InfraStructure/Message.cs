namespace Eventhat.InfraStructure;

public class Message<TData>
{
    public Message(Guid id, Metadata meta, TData data)
    {
        Id = id;
        Data = data;
        Meta = meta;
    }

    public Guid Id { get; }
    public TData Data { get; }
    public Metadata Meta { get; }
}

public class Metadata
{
    public Metadata(Guid traceId, Guid userId)
    {
        TraceId = traceId;
        UserId = userId;
    }

    public Guid TraceId { get; }
    public Guid UserId { get; }
}