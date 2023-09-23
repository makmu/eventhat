namespace Eventhat.Messages.Events;

internal class Sent
{
    public Sent(Guid emailId, string to, string subject, string text, string html)
    {
        EmailId = emailId;
        To = to;
        Subject = subject;
        Text = text;
        Html = html;
    }

    public Guid EmailId { get; }
    public string To { get; }
    public string Subject { get; }
    public string Text { get; }
    public string Html { get; }
}