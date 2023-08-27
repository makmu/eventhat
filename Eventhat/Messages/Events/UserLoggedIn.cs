namespace Eventhat.Messages.Events;

internal class UserLoggedIn
{
    public UserLoggedIn(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}