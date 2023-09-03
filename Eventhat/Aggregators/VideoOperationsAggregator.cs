using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class VideoOperationsAggregator : IAgent
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _subscription;

    public VideoOperationsAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
        _subscription = messageStore.CreateSubscription(
            "videoPublishing",
            "aggregators:video-operations");
        _subscription.RegisterHandler<VideoNamed>(VideoNamedAsync);
        _subscription.RegisterHandler<VideoNameRejected>(VideoNameRejectedAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task VideoNameRejectedAsync(Message<VideoNameRejected> message)
    {
        await _db.InsertVideoOperationAsync(message.Metadata.TraceId, message.StreamName.ToId(), false, message.Data.Reason);
    }

    public async Task VideoNamedAsync(Message<VideoNamed> message)
    {
        await _db.InsertVideoOperationAsync(message.Metadata.TraceId, message.StreamName.ToId(), true, string.Empty);
    }
}