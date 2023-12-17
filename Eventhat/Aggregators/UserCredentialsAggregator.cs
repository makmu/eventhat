using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class UserCredentialsAggregator : IAgent
{
    private readonly MessageSubscription _subscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public UserCredentialsAggregator(IDbContextFactory<ViewDataContext> viewDataDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataDb;
        _subscription = messageStore.CreateSubscription(
            "identity",
            "aggregators:user-credentials");
        _subscription.RegisterHandler<Registered>(RegisteredAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task RegisteredAsync(Message<Registered> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();

        if (viewData.UserCredentials.All(x => x.Id != message.Data.UserId))
        {
            await viewData.UserCredentials.AddAsync(new UserCredentials
            {
                Id = message.Data.UserId,
                Email = message.Data.Email,
                PasswordHash = message.Data.PasswordHash
            });
            await viewData.SaveChangesAsync();
        }
    }
}