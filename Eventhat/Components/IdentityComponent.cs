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
        return await _messageStore.FetchAsync<Identity>($"identity-{identityId}");
    }


    private async Task RegisterAsync(Message<Register> command)
    {
        try
        {
            var identity = await LoadIdentityAsync(command.Data.UserId);
            await EnsureNotRegisteredAsync(identity);
            await WriteRegisteredEventAsync(command);
        }
        catch (AlreadyRegisteredException)
        {
            // do nothing
        }
    }

    private async Task WriteRegisteredEventAsync(Message<Register> command)
    {
        await _messageStore.WriteAsync(
            $"identity-{command.Data.UserId}",
            command.Metadata,
            new Registered(command.Data.UserId, command.Data.Email, command.Data.PasswordHash));
    }

    private Task EnsureNotRegisteredAsync(Identity identity)
    {
        if (identity.IsRegistered) throw new AlreadyRegisteredException();
        return Task.CompletedTask;
    }

    private async Task RegisteredAsync(Message<Registered> @event)
    {
        try
        {
            var identity = await LoadIdentityAsync(@event.Data.UserId);
            EnsureRegistrationEmailNotSent(identity);
            var (to, subject, text, html) = RenderRegistrationEmail(identity);
            await WriteSendCommandAsync(@event, identity, to, subject, text, html);
        }
        catch (AlreadySentRegistrationEmailException)
        {
            // to nothing
        }
    }

    private (string to, string subject, string text, string html) RenderRegistrationEmail(Identity identity)
    {
        return (identity.Email, "Welcome at Eventhat", $"Hi {identity.Email}, welcome to Eventhat!", $"<h1>Hi {identity.Email}, welcome to Eventhat!</h1>");
    }

    private async Task WriteSendCommandAsync(Message<Registered> @event, Identity identity, string to, string subject, string text, string html)
    {
        var emailId = Guid.NewGuid();
        await _messageStore.WriteAsync(
            $"sendEmail:command-{emailId}",
            new Metadata(@event.Metadata.TraceId, @event.Metadata.UserId, $"identity-{identity.Id}"),
            new Send(emailId, to, subject, text, html));
    }

    private async Task SentAsync(Message<Sent> @event)
    {
        var identityId = @event.Metadata.OriginStreamName!.ToId();

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

    private async Task WriteRegistrationEmailSentEventAsync(Message<Sent> @event, Identity identity)
    {
        await _messageStore.WriteAsync(
            @event.Metadata.OriginStreamName!,
            new Metadata(@event.Metadata.TraceId, @event.Metadata.UserId),
            new RegistrationEmailSent(identity.Id, @event.Data.EmailId));
    }
}