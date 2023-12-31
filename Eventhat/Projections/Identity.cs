using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Identity : ProjectionBase
{
    public Identity()
    {
        RegisterEventHandler<Registered>(ApplyRegistered);
        RegisterEventHandler<RegistrationEmailSent>(ApplyRegistrationEmailSent);
    }

    public Guid Id { get; set; } = Guid.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsRegistered { get; private set; }
    public bool RegistrationEmailSent { get; private set; }

    private void ApplyRegistrationEmailSent(Message<RegistrationEmailSent> message)
    {
        RegistrationEmailSent = true;
    }

    private void ApplyRegistered(Message<Registered> message)
    {
        Id = message.Data.UserId;
        Email = message.Data.Email;
        IsRegistered = true;
    }
}