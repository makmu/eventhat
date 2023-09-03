using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class UserCredentialsAggregator : IAgent
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _subscription;

    public UserCredentialsAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
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

    public async Task RegisteredAsync(Message<Registered> message)
    {
        await _db.InsertUserCredentialAsync(message.Data.UserId, message.Data.Email, message.Data.PasswordHash);
    }
}