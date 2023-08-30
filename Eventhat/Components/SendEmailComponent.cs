using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Mail;
using Eventhat.Messages.Commands;
using Eventhat.Messages.Events;
using Eventhat.Projections;

namespace Eventhat.Components;

public class SendEmailComponent : IAgent
{
    private readonly MessageSubscription _subscription;

    public SendEmailComponent(MessageStore messageStore, Mailer mailer, string systemSenderEmailAddress)
    {
        var mailCommandHandlers = new Handlers(messageStore, mailer, systemSenderEmailAddress);
        _subscription = messageStore.CreateSubscription(
            "sendEmail:command",
            mailCommandHandlers.AsDictionary(),
            "components:send-email"
        );
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
        private readonly Mailer _mailer;
        private readonly MessageStore _messageStore;
        private readonly string _systemSenderEmailAddress;

        public Handlers(MessageStore messageStore, Mailer mailer, string systemSenderEmailAddress)
        {
            _messageStore = messageStore;
            _mailer = mailer;
            _systemSenderEmailAddress = systemSenderEmailAddress;
        }

        public Dictionary<Type, Func<MessageEntity, Task>> AsDictionary()
        {
            return new Dictionary<Type, Func<MessageEntity, Task>> { { typeof(Send), SendEmailAsync } };
        }

        private async Task SendEmailAsync(MessageEntity command)
        {
            try
            {
                var email = await LoadEmailAsync(command);
                EnsureEmailHasNotBeenSent(email);
                await SendAsync(command);
                await WriteSentEventAsync(command);
            }
            catch (AlreadySentException)
            {
                // to nothing
            }
            catch (SendException e)
            {
                await WriteFailedEventAsync(command, e);
            }
        }

        private async Task WriteFailedEventAsync(MessageEntity command, SendException exception)
        {
            var data = command.Data.Deserialize<Send>();
            var metadata = command.Metadata.Deserialize<Metadata>();

            var streamName = $"sendEmail-{data.EmailId}";

            await _messageStore.WriteAsync(streamName,
                new Message<Failed>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId, metadata.OriginStreamName),
                    new Failed(data.EmailId, data.To, data.Subject, data.Text, data.Html, exception.Message)));
        }

        private async Task WriteSentEventAsync(MessageEntity command)
        {
            var data = command.Data.Deserialize<Send>();
            var metadata = command.Metadata.Deserialize<Metadata>();

            var streamName = $"sendEmail-{data.EmailId}";

            await _messageStore.WriteAsync(streamName,
                new Message<Sent>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId, metadata.OriginStreamName), new Sent(data.EmailId, data.To, data.Subject, data.Text, data.Html)));
        }

        private async Task SendAsync(MessageEntity sendCommand)
        {
            var data = sendCommand.Data.Deserialize<Send>();
            await _mailer.JustSendItAsync(_systemSenderEmailAddress, data.To, data.Subject, data.Text, data.Html);
        }

        private void EnsureEmailHasNotBeenSent(Email email)
        {
            if (email.IsSent) throw new AlreadySentException();
        }

        private async Task<Email> LoadEmailAsync(MessageEntity messageEntity)
        {
            var command = messageEntity.Data.Deserialize<Send>();
            var emailProjection = new Email.Projection();
            return await _messageStore.FetchAsync($"sendEmail-{command.EmailId}", emailProjection.AsDictionary());
        }
    }
}