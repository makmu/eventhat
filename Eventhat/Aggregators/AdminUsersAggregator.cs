using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class AdminUsersAggregator : IAgent
{
    private readonly MessageSubscription _authenticationSubscription;
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _identitySubscription;

    public AdminUsersAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
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
        await _db.InsertAdminUsersAsync(message.Data.IdentityId);
        await _db.MarkRegistrationEmailSent(message.Data.IdentityId, message.GlobalPosition);
    }

    private async Task RegisteredAsync(Message<Registered> message)
    {
        await _db.InsertAdminUsersAsync(message.Data.UserId);

        await _db.SetAdminUserEmail(message.Data.UserId, message.Data.Email, message.GlobalPosition);
    }

    private async Task UserLoggedInAsync(Message<UserLoggedIn> message)
    {
        await _db.InsertAdminUsersAsync(message.Data.UserId);

        await _db.IncreaseAdminUserLoginCount(message.Data.UserId, message.GlobalPosition);
    }
}