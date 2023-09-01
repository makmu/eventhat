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

    private async Task VideoNameRejectedAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<VideoNameRejected>();
        var metadata = message.Metadata.Deserialize<Metadata>();
        await _db.InsertVideoOperationAsync(metadata.TraceId, message.StreamName.ToId(), false, data.Reason);
    }

    public async Task VideoNamedAsync(MessageEntity message)
    {
        var metadata = message.Metadata.Deserialize<Metadata>();
        await _db.InsertVideoOperationAsync(metadata.TraceId, message.StreamName.ToId(), true, string.Empty);
    }
}