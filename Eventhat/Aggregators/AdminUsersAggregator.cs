using Eventhat.Database;
using Eventhat.Helpers;
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

    private async Task RegistrationEmailSentAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<RegistrationEmailSent>();
        await _db.InsertAdminUsersAsync(data.IdentityId);
        await _db.MarkRegistrationEmailSent(data.IdentityId, message.GlobalPosition);
    }

    private async Task RegisteredAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<Registered>();
        await _db.InsertAdminUsersAsync(data.UserId);

        await _db.SetAdminUserEmail(data.UserId, data.Email, message.GlobalPosition);
    }

    private async Task UserLoggedInAsync(MessageEntity message)
    {
        var data = message.Data.Deserialize<UserLoggedIn>();
        await _db.InsertAdminUsersAsync(data.UserId);

        await _db.IncreaseAdminUserLoginCount(data.UserId, message.GlobalPosition);
    }


    public async Task VideoNamedAsync(MessageEntity message)
    {
        var metadata = message.Metadata.Deserialize<Metadata>();
        await _db.InsertVideoOperationAsync(metadata.TraceId, message.StreamName.ToId(), true, string.Empty);
    }
}