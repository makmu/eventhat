namespace Eventhat.Database.Entities;

public class AdminUser
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool RegistrationEmailSent { get; set; }
    public int LastIdentityEventGlobalPosition { get; set; }
    public int LoginCount { get; set; }
    public int LastAuthenticationEventGlobalPosition { get; set; }
}