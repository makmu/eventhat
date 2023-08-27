using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Commands;
using Eventhat.Messages.Events;
using Eventhat.Projections;

namespace Eventhat.Components;

public class IdentityComponent : IAgent
{
    private readonly MessageSubscription _identityCommandSubscription;

    public IdentityComponent(MessageStore messageStore)
    {
        var identityCommandHandlers = new Handlers(messageStore);
        _identityCommandSubscription = messageStore.CreateSubscription(
            "identity:command",
            identityCommandHandlers.AsDictionary(),
            "components:identity:command"
        );
    }

    public async Task StartAsync()
    {
        await _identityCommandSubscription.StartAsync();
    }

    public void Stop()
    {
        _identityCommandSubscription.Stop();
    }

    public class Handlers
    {
        private readonly MessageStore _messageStore;

        public Handlers(MessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public Dictionary<Type, Func<MessageEntity, Task>> AsDictionary()
        {
            return new Dictionary<Type, Func<MessageEntity, Task>> { { typeof(Register), RegisterAsync } };
        }

        private async Task RegisterAsync(MessageEntity command)
        {
            try
            {
                var data = command.Data.Deserialize<Register>();
                var identity = await LoadIdentityAsync(data.UserId);
                await EnsureNotRegisteredAsync(identity);
                await WriteRegisteredEventAsync(command);
            }
            catch (AlreadyRegisteredException)
            {
                // do nothing
            }
        }

        private async Task WriteRegisteredEventAsync(MessageEntity command)
        {
            var data = command.Data.Deserialize<Register>();
            var registeredEvent = new Message<Registered>(Guid.NewGuid(), command.Metadata.Deserialize<Metadata>(), new Registered(data.UserId, data.Email, data.PasswordHash));
            var identityStreamName = $"identity-{data.UserId}";
            await _messageStore.WriteAsync(identityStreamName, registeredEvent);
        }

        private Task EnsureNotRegisteredAsync(Identity identity)
        {
            if (identity.IsRegistered) throw new AlreadyRegisteredException();
            return Task.CompletedTask;
        }

        private async Task<Identity> LoadIdentityAsync(Guid identityId)
        {
            var identityProjection = new Identity.Projection();
            return await _messageStore.FetchAsync($"identity-{identityId}", identityProjection.AsDictionary());
        }
    }
}