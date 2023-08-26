using System.Text.Json;
using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class MessageSubscription
{
    private readonly Dictionary<string, Func<MessageEntity, Task>> _handlers;
    private readonly int _messagesPerTick;
    private readonly int _positionUpdateInterval;
    private readonly Read _read;
    private readonly string _streamName;
    private readonly string _subscriberId;
    private readonly string _subscriberStreamName;
    private readonly int _tickIntervalMs;
    private readonly Write _write;
    private int _currentPosition;
    private bool _keepGoing = true;
    private int _messagesSinceLastPositionWrite;

    public MessageSubscription(
        Read read,
        Write write,
        string streamName,
        Dictionary<string, Func<MessageEntity, Task>> handlers,
        string subscriberId,
        int messagesPerTick,
        int positionUpdateInterval,
        int tickIntervalMs)
    {
        _subscriberStreamName = $"subscriberPosition-{subscriberId}";
        _read = read;
        _write = write;
        _streamName = streamName;
        _handlers = handlers;
        _subscriberId = subscriberId;
        _handlers = handlers;
        _messagesPerTick = messagesPerTick;
        _positionUpdateInterval = positionUpdateInterval;
        _tickIntervalMs = tickIntervalMs;
    }

    public async Task LoadPosition()
    {
        var lastMessage = await _read.ReadLastMessageAsync(_subscriberStreamName);
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
        await _write.WriteAsync(_subscriberStreamName,
            new Message<StreamRead>(Guid.NewGuid(), new Metadata(Guid.Empty, Guid.Empty), new StreamRead(position)));
    }

    public async Task<IEnumerable<MessageEntity>> GetNextBatchOfMessages()
    {
        return await _read.ReadAsync(_streamName, _currentPosition + 1, _messagesPerTick);
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
        if (_handlers.TryGetValue(message.Type, out var handler)) await handler(message);
        await _handlers["$any"](message);
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
            if (messagesProcessed == 0) Thread.Sleep(_tickIntervalMs);
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
            Console.WriteLine($"Error processing batch: {e.Message}");
            Stop();
            return 0;
        }
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