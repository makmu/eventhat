namespace Eventhat.Controllers.Exceptions;

internal class CredentialMismatchException : Exception
{
    public CredentialMismatchException(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}