namespace Eventhat.Mail;

internal class SendException : Exception
{
    public SendException(string message) : base(message)
    {
    }
}