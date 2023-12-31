using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Email : ProjectionBase
{
    public Email()
    {
        RegisterEventHandler<Sent>(ApplySent);
    }

    public bool IsSent { get; private set; }

    private void ApplySent(Message<Sent> message)
    {
        IsSent = true;
    }
}