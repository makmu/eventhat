using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class AdminUsersAggregator : IAgent
{
    private readonly MessageSubscription _authenticationSubscription;
    private readonly MessageSubscription _identitySubscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public AdminUsersAggregator(IDbContextFactory<ViewDataContext> viewDataDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataDb;
        _identitySubscription = messageStore.CreateSubscription(
            "identity",
            "aggregators:identity:admin-users");
        _identitySubscription.RegisterHandler<Registered>(RegisteredAsync);
        _identitySubscription.RegisterHandler<RegistrationEmailSent>(RegistrationEmailSentAsync);

        _authenticationSubscription = messageStore.CreateSubscription(
            "authentication",
            "aggregators:authentication:admin-users");
        _authenticationSubscription.RegisterHandler<UserLoggedIn>(UserLoggedInAsync);
    }

    public void Start()
    {
        _ = _identitySubscription.StartAsync();
        _ = _authenticationSubscription.StartAsync();
    }

    public void Stop()
    {
        _identitySubscription.Stop();
        _authenticationSubscription.Stop();
    }

    private async Task RegistrationEmailSentAsync(Message<RegistrationEmailSent> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        var admin = viewData.AdminUsers.Single(x => x.Id == message.Data.IdentityId);
        admin.RegistrationEmailSent = true;
        admin.LastIdentityEventGlobalPosition = message.GlobalPosition;
        await viewData.SaveChangesAsync();
    }

    private async Task RegisteredAsync(Message<Registered> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        if (viewData.AdminUsers.All(x => x.Id != message.Data.UserId))
        {
            await viewData.AdminUsers.AddAsync(new AdminUser
            {
                Id = message.Data.UserId,
                Email = message.Data.Email,
                RegistrationEmailSent = false,
                LastIdentityEventGlobalPosition = message.GlobalPosition,
                LoginCount = 0,
                LastAuthenticationEventGlobalPosition = 0
            });

            await viewData.SaveChangesAsync();
        }
    }

    private async Task UserLoggedInAsync(Message<UserLoggedIn> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        var admin = viewData.AdminUsers.Single(x => x.Id == message.Data.UserId);
        admin.LoginCount++;
        admin.LastAuthenticationEventGlobalPosition = message.GlobalPosition;

        await viewData.SaveChangesAsync();
    }
}