namespace Eventhat.Messages.Events;

internal class Registered
{
    public Registered(Guid userId, string email, string passwordHash)
    {
        UserId = userId;
        Email = email;
        PasswordHash = passwordHash;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string PasswordHash { get; }
}