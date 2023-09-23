namespace Eventhat.Messages.Commands;

public class PublishVideo
{
    public PublishVideo(Guid videoId, Guid ownerId, Uri sourceUri)
    {
        VideoId = videoId;
        OwnerId = ownerId;
        SourceUri = sourceUri;
    }

    public Guid VideoId { get; set; }
    public Guid OwnerId { get; set; }
    public Uri SourceUri { get; set; }
}