using Eventhat.Database;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Email
{
    public bool IsSent { get; set; }

    public class Projection
    {
        public Dictionary<Type, Func<Email, MessageEntity, Email>> AsDictionary()
        {
            return new Dictionary<Type, Func<Email, MessageEntity, Email>> { { typeof(Sent), Sent } };
        }

        private Email Sent(Email email, MessageEntity message)
        {
            email.IsSent = true;
            return email;
        }
    }
}