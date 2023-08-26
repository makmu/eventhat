using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class MessageStore
{
    private readonly Read _read;

    public MessageStore(IMessageStreamDatabase db)
    {
        Write = new Write(db);
        _read = new Read(db);
    }

    public Write Write { get; }

    public async Task<IEnumerable<MessageEntity>> ReadAsync(string streamName, int fromPosition, int messagesPerTick)
    {
        return await _read.ReadAsync(streamName, fromPosition, messagesPerTick);
    }

    public async Task<MessageEntity?> ReadLastMessageAsync(string streamName)
    {
        return await _read.ReadLastMessageAsync(streamName);
    }

    public MessageSubscription CreateSubscription(
        string streamName,
        Dictionary<string, Func<MessageEntity, Task>> handlers,
        string subscriberId,
        int messagesPerTick = 100,
        int positionUpdateInterval = 100,
        int tickIntervalMs = 100)
    {
        return new MessageSubscription(_read, Write, streamName, handlers, subscriberId, messagesPerTick, positionUpdateInterval, tickIntervalMs);
    }
}