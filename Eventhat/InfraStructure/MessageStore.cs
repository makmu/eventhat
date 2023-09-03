using System.Text.Json;
using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.Projections;

namespace Eventhat.InfraStructure;

public class MessageStore
{
    private readonly IMessageStreamDatabase _db;
    private readonly Dictionary<string, List<MessageSubscription>> _subscriptions = new();

    public MessageStore(IMessageStreamDatabase db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MessageEntity>> ReadAsync(string streamName, int fromPosition = 0, int batchSize = 1000)
    {
        if (streamName == "$all") return _db.Messages.OrderBy(m => m.GlobalPosition).Take(new Range(fromPosition, fromPosition + batchSize));

        if (streamName.Contains('-'))
            return await _db.GetStreamMessages(streamName, fromPosition, batchSize);

        return await _db.GetCategoryMessages(streamName, fromPosition, batchSize);
    }

    public async Task<MessageEntity?> ReadLastMessageAsync(string streamName)
    {
        return await _db.GetLastStreamMessage(streamName);
    }

    public async Task WriteAsync<T>(string streamName, Metadata metadata, T data, int? expectedVersion = null)
    {
        var messageId = Guid.NewGuid();
        await _db.WriteMessageAsync(Guid.NewGuid(), streamName, typeof(T).ToString(), JsonSerializer.Serialize(metadata), JsonSerializer.Serialize(data), expectedVersion);

        if (_subscriptions.TryGetValue(streamName.GetCategory(), out var categorySubscriptions))
            foreach (var subscription in categorySubscriptions)
                subscription.PushLastMessageId(messageId);

        if (_subscriptions.TryGetValue("$all", out var broadcastSubscriptions))
            foreach (var subscription in broadcastSubscriptions)
                subscription.PushLastMessageId(messageId);
    }

    public async Task<T> FetchAsync<T>(string streamName) where T : ProjectionBase, new()
    {
        var messages = await ReadAsync(streamName);
        var projection = new T();
        foreach (var message in messages) projection.ApplyEvent(message.Type, message.Id, message.StreamName, message.Metadata, message.Data, message.Position, message.GlobalPosition);

        return projection;
    }

    public MessageSubscription CreateSubscription(
        string streamCategoryName,
        string subscriberId,
        string? originStreamName = null,
        int messagesPerTick = 100,
        int positionUpdateInterval = 100,
        int tickIntervalMs = 100)
    {
        if (streamCategoryName.Contains('-')) throw new ArgumentException($"invalid stream name {streamCategoryName}: only subscriptions to category are allowed", nameof(streamCategoryName));

        if (!_subscriptions.ContainsKey(streamCategoryName)) _subscriptions.Add(streamCategoryName, new List<MessageSubscription>());

        var subscription = new MessageSubscription(this, streamCategoryName, subscriberId, originStreamName, messagesPerTick, positionUpdateInterval, tickIntervalMs);
        _subscriptions[streamCategoryName].Add(subscription);
        return subscription;
    }
}