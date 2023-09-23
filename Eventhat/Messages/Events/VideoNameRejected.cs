namespace Eventhat.Messages.Events;

public class VideoNameRejected
{
    public VideoNameRejected(string name, string reason)
    {
        Name = name;
        Reason = reason;
    }

    public string Name { get; }
    public string Reason { get; }
}