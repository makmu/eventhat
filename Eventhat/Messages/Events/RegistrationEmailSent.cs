namespace Eventhat.Messages.Events;

internal class RegistrationEmailSent
{
    public RegistrationEmailSent(Guid identityId, Guid emailId)
    {
        IdentityId = identityId;
        EmailId = emailId;
    }

    public Guid IdentityId { get; }
    public Guid EmailId { get; }
}