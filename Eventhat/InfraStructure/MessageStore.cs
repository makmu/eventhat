using System.Text.Json;
using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class MessageStore
{
    private readonly IMessageStreamDatabase _db;

    public MessageStore(IMessageStreamDatabase db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MessageEntity>> ReadAsync(string streamName, int fromPosition = 0, int batchSize = 1000)
    {
        if (streamName.Contains('-'))
            return await _db.GetStreamMessages(streamName, fromPosition, batchSize);

        return await _db.GetCategoryMessages(streamName, fromPosition, batchSize);
    }

    public async Task<MessageEntity?> ReadLastMessageAsync(string streamName)
    {
        return await _db.GetLastStreamMessage(streamName);
    }

    public async Task WriteAsync<T>(string streamName, Message<T> message, int? expectedVersion = null)
    {
        await _db.WriteMessageAsync(message.Id, streamName, typeof(T).ToString(), JsonSerializer.Serialize(message.Data), JsonSerializer.Serialize(message.Meta), expectedVersion);
    }

    public async Task<T> FetchAsync<T>(string streamName, Dictionary<Type, Func<T, MessageEntity, T>> projection) where T : new()
    {
        var messages = await ReadAsync(streamName);
        return ProjectAsync(messages, projection);
    }

    private T ProjectAsync<T>(IEnumerable<MessageEntity> messages, Dictionary<Type, Func<T, MessageEntity, T>> projection) where T : new()
    {
        return messages.Aggregate(new T(), (entity, @event) =>
        {
            if (projection.TryGetValue(@event.Data.GetType(), out var p)) return p(entity, @event);

            return entity;
        });
    }

    public MessageSubscription CreateSubscription(
        string streamName,
        Dictionary<Type, Func<MessageEntity, Task>> handlers,
        string subscriberId,
        string? originStreamName = null,
        int messagesPerTick = 100,
        int positionUpdateInterval = 100,
        int tickIntervalMs = 100)
    {
        return new MessageSubscription(this, streamName, handlers, subscriberId, originStreamName, messagesPerTick, positionUpdateInterval, tickIntervalMs);
    }
}