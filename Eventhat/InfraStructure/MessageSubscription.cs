using System.Text.Json;
using Eventhat.Database;
using Eventhat.Helpers;

namespace Eventhat.InfraStructure;

public class MessageSubscription
{
    private readonly Dictionary<Type, Func<MessageEntity, Task>> _handlers = new();
    private readonly int _messagesPerTick;
    private readonly MessageStore _messageStore;
    private readonly string? _originStreamName;
    private readonly int _positionUpdateInterval;
    private readonly string _streamName;
    private readonly string _subscriberId;
    private readonly string _subscriberStreamName;
    private readonly int _tickIntervalMs;
    private int _currentPosition;
    private bool _keepGoing = true;
    private int _messagesSinceLastPositionWrite;

    public MessageSubscription(MessageStore messageStore,
        string streamName,
        string subscriberId,
        string? originStreamName,
        int messagesPerTick,
        int positionUpdateInterval,
        int tickIntervalMs)
    {
        _subscriberStreamName = $"subscriberPosition-{subscriberId}";
        _messageStore = messageStore;
        _streamName = streamName;
        _subscriberId = subscriberId;
        _originStreamName = originStreamName;
        _messagesPerTick = messagesPerTick;
        _positionUpdateInterval = positionUpdateInterval;
        _tickIntervalMs = tickIntervalMs;
    }

    public async Task LoadPosition()
    {
        var lastMessage = await _messageStore.ReadLastMessageAsync(_subscriberStreamName);
        if (lastMessage == null)
        {
            _currentPosition = 0;
            return;
        }

        var updated = JsonSerializer.Deserialize<StreamRead>(lastMessage.Data);
        if (updated == null) throw new Exception($"Invalid subscriber position update message for subscriber stream '{_subscriberStreamName}'");

        _currentPosition = updated.Position;
    }

    public async Task UpdateReadPosition(int position)
    {
        _currentPosition = position;
        _messagesSinceLastPositionWrite += 1;
        if (_messagesSinceLastPositionWrite == _positionUpdateInterval)
        {
            _messagesSinceLastPositionWrite = 0;
            await WritePosition(position);
        }
    }

    public async Task WritePosition(int position)
    {
        await _messageStore.WriteAsync(
            _subscriberStreamName,
            new Metadata(Guid.Empty, Guid.Empty),
            new StreamRead(position));
    }

    public async Task<IEnumerable<MessageEntity>> GetNextBatchOfMessages()
    {
        var batch = await _messageStore.ReadAsync(_streamName, _currentPosition + 1, _messagesPerTick);
        return batch.Where(x => _originStreamName == null || x.Metadata.Deserialize<Metadata>().OriginStreamName?.GetCategory() == _originStreamName);
    }

    public async Task<int> ProcessBatch(IEnumerable<MessageEntity> messages)
    {
        var messagesAsList = messages.ToList();
        foreach (var message in messagesAsList)
        {
            await HandleMessage(message);
            await UpdateReadPosition(message.GlobalPosition);
        }

        return messagesAsList.Count;
    }

    private async Task HandleMessage(MessageEntity message)
    {
        var type = Type.GetType(message.Type);
        if (type == null) throw new Exception($"Unknown message type {message.Type}");
        if (_handlers.TryGetValue(type, out var handler))
            await handler(message);
        if (_handlers.TryGetValue(typeof(object), out handler))
            await handler(message);
    }

    public async Task StartAsync()
    {
        Console.WriteLine($"Started {_subscriberId}");

        await PollAsync();
    }

    public void Stop()
    {
        Console.WriteLine($"Stopped {_subscriberId}");
        _keepGoing = false;
    }

    private async Task PollAsync()
    {
        await LoadPosition();

        while (_keepGoing)
        {
            var messagesProcessed = await TickAsync();
            if (messagesProcessed == 0) await Task.Delay(_tickIntervalMs);
        }
    }

    private async Task<int> TickAsync()
    {
        try
        {
            return await ProcessBatch(await GetNextBatchOfMessages());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error processing batch: {e.Message} {e.StackTrace}");
            Stop();
            return 0;
        }
    }

    public void RegisterHandler<T>(Func<Message<T>, Task> handler)
    {
        _handlers.Add(typeof(T), x => handler(new Message<T>(x.Id, x.StreamName, x.Metadata.Deserialize<Metadata>(), x.Data.Deserialize<T>(), x.Position, x.GlobalPosition)));
    }
}

public class StreamRead
{
    public StreamRead(int position)
    {
        Position = position;
    }

    public int Position { get; }
}