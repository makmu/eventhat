using System.Text.Json;
using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.Projections;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.InfraStructure;

public class MessageStore
{
    private readonly IDbContextFactory<MessageContext> _messageDbContextDb;
    private readonly Dictionary<string, List<MessageSubscription>> _subscriptions = new();

    public MessageStore(IDbContextFactory<MessageContext> messageDbContextDb)
    {
        _messageDbContextDb = messageDbContextDb;
    }

    public Task<IEnumerable<MessageEntity>> ReadAsync(string streamName, int fromPosition = 0, int batchSize = 1000)
    {
        using var messageDb = _messageDbContextDb.CreateDbContext();

        if (streamName == "$all")
            return Task.FromResult<IEnumerable<MessageEntity>>(
                messageDb.Messages
                    .OrderBy(m => m.GlobalPosition)
                    .Skip(fromPosition - 1)
                    .Take(batchSize)
                    .ToList());

        if (streamName.Contains('-'))
            return Task.FromResult<IEnumerable<MessageEntity>>(
                messageDb.Messages
                    .Where(m => m.StreamName == streamName && m.Position >= fromPosition)
                    .OrderBy(m => m.Position)
                    .Take(batchSize)
                    .ToList());

        return Task.FromResult<IEnumerable<MessageEntity>>(
            messageDb.Messages
                .Where(m => m.StreamName.Contains($"{streamName}-") && m.GlobalPosition >= fromPosition)
                .OrderBy(m => m.GlobalPosition)
                .Take(batchSize)
                .ToList());
    }

    public Task<MessageEntity?> ReadLastMessageAsync(string streamName)
    {
        using var messageDb = _messageDbContextDb.CreateDbContext();
        return Task.FromResult(messageDb.Messages.Where(m => m.StreamName == streamName).ToList().MaxBy(m => m.Position));
    }

    public async Task WriteAsync<T>(string streamName, Metadata metadata, T data, int? expectedVersion = null)
    {
        using var messageDb = _messageDbContextDb.CreateDbContext();

        var messageId = Guid.NewGuid();
        await messageDb.Messages.AddAsync(
            new MessageEntity
            {
                Id = Guid.NewGuid(),
                StreamName = streamName,
                Type = typeof(T).ToString(),
                Position = expectedVersion ?? default,
                Data = JsonSerializer.Serialize(data),
                Metadata = JsonSerializer.Serialize(metadata),
                Time = DateTimeOffset.Now
            });

        await messageDb.SaveChangesAsync();

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