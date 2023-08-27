namespace Eventhat.Database.Entities;

public class UserCredentials
{
    public UserCredentials(Guid id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Guid Id { get; }
    public string Email { get; }
    public string PasswordHash { get; }
}