namespace Eventhat.Messages.Events;

public class NameVideo
{
    public NameVideo(Guid videoId, string name)
    {
        VideoId = videoId;
        Name = name;
    }

    public Guid VideoId { get; }
    public string Name { get; }
}