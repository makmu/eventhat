namespace Eventhat.Mail;

public class Mailer
{
    public Task JustSendItAsync(string from, string to, string subject, string text, string html)
    {
        if (to.Contains("unreachable.com")) throw new SendException("Domain 'unreachable.com' not reachable");

        Console.WriteLine("(Not actually) sending email....");
        Console.WriteLine($"  from: {from}");
        Console.WriteLine($"  to: {to}");
        Console.WriteLine($"  subject: {subject}");
        Console.WriteLine($"  text: {text}");
        Console.WriteLine($"  html: {html}");
        return Task.CompletedTask;
    }
}