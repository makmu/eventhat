namespace Eventhat.Messages.Events;

public class VideoPublishingFailed
{
    public VideoPublishingFailed(Guid videoId, Guid ownerId, Uri sourceUri, string reason)
    {
        VideoId = videoId;
        OwnerId = ownerId;
        SourceUri = sourceUri;
        Reason = reason;
    }

    public Guid VideoId { get; set; }
    public Guid OwnerId { get; set; }
    public Uri SourceUri { get; set; }
    public string Reason { get; }
}