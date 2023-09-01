namespace Eventhat.Messages.Events;

public class VideoNamed
{
    public VideoNamed(string name)
    {
        Name = name;
    }

    public string Name { get; }
}