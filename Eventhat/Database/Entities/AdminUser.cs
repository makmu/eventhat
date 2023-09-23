namespace Eventhat.Database.Entities;

public class AdminUser
{
    public AdminUser(
        Guid id,
        string email,
        bool registrationEmailSent,
        int lastIdentityEventGlobalPosition,
        int loginCount,
        int lastAuthenticationEventGlobalPosition)
    {
        Id = id;
        Email = email;
        RegistrationEmailSent = registrationEmailSent;
        LastIdentityEventGlobalPosition = lastIdentityEventGlobalPosition;
        LoginCount = loginCount;
        LastAuthenticationEventGlobalPosition = lastAuthenticationEventGlobalPosition;
    }

    public Guid Id { get; }
    public string Email { get; }
    public bool RegistrationEmailSent { get; }
    public int LastIdentityEventGlobalPosition { get; }
    public int LoginCount { get; }
    public int LastAuthenticationEventGlobalPosition { get; }
}