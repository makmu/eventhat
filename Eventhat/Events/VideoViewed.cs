namespace Eventhat.Events;

public class VideoViewed
{
    public Guid TraceId { get; }
    public Guid UserId { get; }
    public Guid VideoId { get; }

    public VideoViewed(Guid traceId, Guid userId, Guid videoId)
    {
        TraceId = traceId;
        UserId = userId;
        VideoId = videoId;
    }
}