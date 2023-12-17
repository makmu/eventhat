using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class AdminStreamsAggregator : IAgent
{
    private readonly MessageSubscription _subscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public AdminStreamsAggregator(IDbContextFactory<ViewDataContext> viewDataDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataDb;
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

    private async Task AnyMessageAsync(Message<object> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        var existingStream = viewData.AdminStreams.SingleOrDefault(s => s.StreamName == message.StreamName);
        if (existingStream == null)
        {
            viewData.AdminStreams.Add(new AdminStream
            {
                StreamName = message.StreamName,
                MessageCount = 1,
                LastMessageId = message.Id,
                LastMessageGlobalPosition = message.GlobalPosition
            });
        }
        else if (existingStream.LastMessageGlobalPosition < message.GlobalPosition)
        {
            existingStream.MessageCount++;
            existingStream.LastMessageGlobalPosition = message.GlobalPosition;
        }

        await viewData.SaveChangesAsync();
    }
}