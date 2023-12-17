using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class CreatorsVideosAggregator : IAgent
{
    private readonly MessageSubscription _subscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public CreatorsVideosAggregator(IDbContextFactory<ViewDataContext> viewDataDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataDb;
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

    private async Task VideoPublishedAsync(Message<VideoPublished> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        if (viewData.CreatorVideos.All(x => x.Id != message.Data.VideoId))
        {
            await viewData.AddAsync(new CreatorVideo
            {
                Id = message.Data.VideoId,
                TranscodedUri = message.Data.TranscodedUri,
                Name = "Untitled",
                Position = message.Position
            });
            await viewData.SaveChangesAsync();
        }
    }

    public async Task VideoNamedAsync(Message<VideoNamed> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        var video = viewData.CreatorVideos.SingleOrDefault(v => v.Id == message.StreamName.ToId() && v.Position < message.Position);
        if (video == null)
            return;

        video.Name = message.Data.Name;
        video.Position = message.Position;

        await viewData.SaveChangesAsync();
    }
}