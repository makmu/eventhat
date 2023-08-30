using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class UserCredentialsAggregator : IAgent
{
    private readonly Queries _queries;
    private readonly MessageSubscription _subscription;

    public UserCredentialsAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _queries = new Queries(db);
        var handlers = new Handlers(_queries);
        _subscription = messageStore.CreateSubscription(
            "identity",
            handlers.AsDictionary(),
            "aggregators:user-credentials");
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    public class Handlers
    {
        private readonly Queries _queries;

        public Handlers(Queries queries)
        {
            _queries = queries;
        }

        public async Task RegisteredAsync(MessageEntity message)
        {
            var data = message.Data.Deserialize<Registered>();
            await _queries.CreateUserCredentialsAsync(data.UserId, data.Email, data.PasswordHash);
        }

        public Dictionary<Type, Func<MessageEntity, Task>> AsDictionary()
        {
            return new Dictionary<Type, Func<MessageEntity, Task>> { { typeof(Registered), RegisteredAsync } };
        }
    }

    public class Queries
    {
        private readonly IMessageStreamDatabase _db;

        public Queries(IMessageStreamDatabase db)
        {
            _db = db;
        }

        public async Task CreateUserCredentialsAsync(Guid id, string email, string passwordHash)
        {
            await _db.InsertUserCredentialAsync(id, email, passwordHash);
        }
    }
}