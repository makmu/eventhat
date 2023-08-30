namespace Eventhat.Messages.Events;

internal class Failed
{
    public Failed(Guid emailId, string to, string subject, string text, string html, string reason)
    {
        EmailId = emailId;
        To = to;
        Subject = subject;
        Text = text;
        Html = html;
        Reason = reason;
    }

    public Guid EmailId { get; }
    public string To { get; }
    public string Subject { get; }
    public string Text { get; }
    public string Html { get; }
    public string Reason { get; }
}