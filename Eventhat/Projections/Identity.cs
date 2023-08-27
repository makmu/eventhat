using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Identity
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }

    public class Projection
    {
        public Dictionary<Type, Func<Identity, MessageEntity, Identity>> AsDictionary()
        {
            return new Dictionary<Type, Func<Identity, MessageEntity, Identity>> { { typeof(Registered), Registered } };
        }

        private Identity Registered(Identity identity, MessageEntity message)
        {
            var registered = message.Data.Deserialize<Registered>();
            identity.Id = registered.Id;
            identity.Email = registered.Email;
            identity.IsRegistered = true;
            return identity;
        }
    }
}