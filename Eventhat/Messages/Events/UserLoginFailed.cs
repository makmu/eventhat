namespace Eventhat.Messages.Events;

internal class UserLoginFailed
{
    public UserLoginFailed(Guid userId, string reason)
    {
        UserId = userId;
        Reason = reason;
    }

    public Guid UserId { get; }
    public string Reason { get; }
}