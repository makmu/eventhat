namespace Eventhat.Database.Entities;

public class UserCredentials
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}