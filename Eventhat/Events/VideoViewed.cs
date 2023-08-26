namespace Eventhat.Events;

public class VideoViewed
{
    public VideoViewed(Guid userId, Guid videoId)
    {
        UserId = userId;
        VideoId = videoId;
    }

    public Guid UserId { get; }
    public Guid VideoId { get; }
}