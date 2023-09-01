using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class CreatorsVideosAggregator : IAgent
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _subscription;

    public CreatorsVideosAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
        _subscription = messageStore.CreateSubscription(
            "videoPublishing",
            "aggregators:creators-videos");
        _subscription.RegisterHandler<VideoPublished>(VideoPublishedAsync);
        _subscription.RegisterHandler<VideoNamed>(VideoNamedAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task VideoPublishedAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<VideoPublished>();
        await _db.InsertCreatorsVideoAsync(data.VideoId, data.TranscodedUri, message.Position);
    }

    public async Task VideoNamedAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<VideoNamed>();
        await _db.UpdateCreatorsVideoAsync(message.StreamName.ToId(), data.Name, message.Position);
    }
}