using Eventhat.Database;
using Eventhat.InfraStructure;

namespace Eventhat.Aggregators;

public class AdminStreamsAggregator : IAgent
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _subscription;

    public AdminStreamsAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
        _subscription = messageStore.CreateSubscription(
            "$all",
            "aggregators:admin-streams");
        _subscription.RegisterHandler<object>(AnyMessageAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task AnyMessageAsync(MessageEntity message)
    {
        await _db.UpsertStream(message.StreamName, message.Id, message.GlobalPosition);
    }
}