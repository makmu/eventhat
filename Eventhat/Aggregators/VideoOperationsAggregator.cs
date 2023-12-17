using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class VideoOperationsAggregator : IAgent
{
    private readonly MessageSubscription _subscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public VideoOperationsAggregator(IDbContextFactory<ViewDataContext> viewDataDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataDb;
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
        using var viewData = _viewDataDb.CreateDbContext();
        if (viewData.VideoOperations.All(x => x.TraceId != message.Metadata.TraceId))
        {
            await viewData.VideoOperations.AddAsync(new VideoOperation
            {
                TraceId = message.Metadata.TraceId,
                VideoId = message.StreamName.ToId(),
                Succeeded = false,
                FailureReason = message.Data.Reason
            });
            await viewData.SaveChangesAsync();
        }
    }

    private async Task VideoNamedAsync(Message<VideoNamed> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        if (viewData.VideoOperations.All(x => x.TraceId != message.Metadata.TraceId))
        {
            await viewData.VideoOperations.AddAsync(new VideoOperation
            {
                TraceId = message.Metadata.TraceId,
                VideoId = message.StreamName.ToId(),
                Succeeded = true,
                FailureReason = string.Empty
            });
            await viewData.SaveChangesAsync();
        }
    }
}