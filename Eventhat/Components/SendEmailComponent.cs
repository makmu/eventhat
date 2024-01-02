using Eventhat.InfraStructure;
using Eventhat.Mail;
using Eventhat.Messages.Commands;
using Eventhat.Messages.Events;
using Eventhat.Projections;

namespace Eventhat.Components;

public class SendEmailComponent : IAgent
{
    private readonly Mailer _mailer;
    private readonly MessageStore _messageStore;
    private readonly MessageSubscription _subscription;
    private readonly string _systemSenderEmailAddress;

    public SendEmailComponent(MessageStore messageStore, Mailer mailer, string systemSenderEmailAddress)
    {
        _messageStore = messageStore;
        _mailer = mailer;
        _systemSenderEmailAddress = systemSenderEmailAddress;
        _subscription = messageStore.CreateSubscription(
            "sendEmail:command",
            "components:send-email"
        );
        _subscription.RegisterHandler<Send>(SendEmailAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task SendEmailAsync(Message<Send> command)
    {
        try
        {
            var email = await LoadEmailAsync(command.Data.EmailId);
            EnsureEmailHasNotBeenSent(email);
            await SendAsync(command.Data);
            await WriteSentEventAsync(command);
        }
        catch (AlreadySentException)
        {
            // do nothing
        }
        catch (SendException e)
        {
            await WriteFailedEventAsync(command, e);
        }
    }

    private async Task WriteFailedEventAsync(Message<Send> command, SendException exception)
    {
        await _messageStore.WriteAsync(
            $"sendEmail-{command.Data.EmailId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId, command.Metadata.OriginStreamName),
            new Failed(command.Data.EmailId, command.Data.To, command.Data.Subject, command.Data.Text, command.Data.Html, exception.Message));
    }

    private async Task WriteSentEventAsync(Message<Send> command)
    {
        await _messageStore.WriteAsync(
            $"sendEmail-{command.Data.EmailId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId, command.Metadata.OriginStreamName),
            new Sent(command.Data.EmailId, command.Data.To, command.Data.Subject, command.Data.Text, command.Data.Html));
    }

    private async Task SendAsync(Send data)
    {
        await _mailer.JustSendItAsync(_systemSenderEmailAddress, data.To, data.Subject, data.Text, data.Html);
    }

    private void EnsureEmailHasNotBeenSent(Email email)
    {
        if (email.IsSent) throw new AlreadySentException();
    }

    private async Task<Email> LoadEmailAsync(Guid emailId)
    {
        return await _messageStore.FetchAsync<Email>($"sendEmail-{emailId}");
    }
}