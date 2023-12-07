using Eventhat.Components.Exceptions;
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
    private readonly MessageSubscription _identityEventSubscription;
    private readonly MessageStore _messageStore;
    private readonly MessageSubscription _sendEmailEventSubscription;

    public IdentityComponent(MessageStore messageStore)
    {
        _messageStore = messageStore;
        _identityCommandSubscription = messageStore.CreateSubscription(
            "identity:command",
            "components:identity:command"
        );
        _identityCommandSubscription.RegisterHandler<Register>(RegisterAsync);

        _identityEventSubscription = messageStore.CreateSubscription(
            "identity",
            "components:identity");
        _identityEventSubscription.RegisterHandler<Registered>(RegisteredAsync);

        _sendEmailEventSubscription = messageStore.CreateSubscription(
            "sendEmail",
            "components:identity:sendEmailEvents",
            "identity");
        _sendEmailEventSubscription.RegisterHandler<Sent>(SentAsync);
    }

    public void Start()
    {
        _ = _identityCommandSubscription.StartAsync();
        _ = _identityEventSubscription.StartAsync();
        _ = _sendEmailEventSubscription.StartAsync();
    }

    public void Stop()
    {
        _sendEmailEventSubscription.Stop();
        _identityEventSubscription.Stop();
        _identityCommandSubscription.Stop();
    }

    private void EnsureRegistrationEmailNotSent(Identity identity)
    {
        if (identity.RegistrationEmailSent) throw new AlreadySentRegistrationEmailException();
    }


    private async Task<Identity> LoadIdentityAsync(Guid identityId)
    {
        var identityProjection = new Identity.Projection();
        return await _messageStore.FetchAsync($"identity-{identityId}", identityProjection.AsDictionary());
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

    private async Task RegisteredAsync(MessageEntity @event)
    {
        try
        {
            var data = @event.Data.Deserialize<Registered>();
            var identity = await LoadIdentityAsync(data.UserId);
            EnsureRegistrationEmailNotSent(identity);
            var (to, subject, text, html) = RenderRegistrationEmail(identity);
            await WriteSendCommandAsync(@event, identity, to, subject, text, html);
        }
        catch (AlreadySentRegistrationEmailException e)
        {
            // to nothing
        }
    }

    private (string to, string subject, string text, string html) RenderRegistrationEmail(Identity identity)
    {
        return (identity.Email, "Welcome at Eventhat", $"Hi {identity.Email}, welcome to Eventhat!", $"<h1>Hi {identity.Email}, welcome to Eventhat!</h1>");
    }

    private async Task WriteSendCommandAsync(MessageEntity @event, Identity identity, string to, string subject, string text, string html)
    {
        var emailId = Guid.NewGuid();
        var metadata = @event.Metadata.Deserialize<Metadata>();
        var streamName = $"sendEmail:command-{emailId}";
        await _messageStore.WriteAsync(streamName,
            new Message<Send>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId, $"identity-{identity.Id}"), new Send(emailId, to, subject, text, html)));
    }

    private async Task SentAsync(MessageEntity @event)
    {
        var metadata = @event.Metadata.Deserialize<Metadata>();
        var identityId = metadata.OriginStreamName!.ToId();

        try
        {
            var identity = await LoadIdentityAsync(identityId);
            EnsureRegistrationEmailNotSent(identity);
            await WriteRegistrationEmailSentEventAsync(@event, identity);
        }
        catch (AlreadySentRegistrationEmailException)
        {
            // do nothing
        }
    }

    private async Task WriteRegistrationEmailSentEventAsync(MessageEntity @event, Identity identity)
    {
        var metadata = @event.Metadata.Deserialize<Metadata>();
        var data = @event.Data.Deserialize<Sent>();

        var identityStreamName = metadata.OriginStreamName!;

        await _messageStore.WriteAsync(identityStreamName,
            new Message<RegistrationEmailSent>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId), new RegistrationEmailSent(identity.Id, data.EmailId)));
    }
}