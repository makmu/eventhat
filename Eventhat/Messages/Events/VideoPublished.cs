namespace Eventhat.Messages.Events;

public class VideoPublished
{
    public VideoPublished(Guid videoId, Guid ownerId, Uri sourceUri, Uri transcodedUri)
    {
        VideoId = videoId;
        OwnerId = ownerId;
        SourceUri = sourceUri;
        TranscodedUri = transcodedUri;
    }

    public Guid VideoId { get; set; }
    public Guid OwnerId { get; set; }
    public Uri SourceUri { get; set; }
    public Uri TranscodedUri { get; set; }
}