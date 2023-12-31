using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Video : ProjectionBase
{
    public Video()
    {
        RegisterEventHandler<VideoPublished>(VideoPublished);
        RegisterEventHandler<VideoPublishingFailed>(VideoPublishingFailed);
        RegisterEventHandler<VideoNamed>(VideoNamed);
        RegisterEventHandler<VideoNameRejected>(VideoNameRejected);
    }

    public Guid Id { get; set; }
    public bool PublishingAttempted { get; set; }
    public Uri? SourceUri { get; set; }
    public Uri? TranscodedUri { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Sequence { get; set; }

    private void VideoNamed(Message<VideoNamed> message)
    {
        Sequence = message.GlobalPosition;
        Name = message.Data.Name;
    }

    private void VideoNameRejected(Message<VideoNameRejected> message)
    {
        Sequence = message.GlobalPosition;
    }

    private void VideoPublished(Message<VideoPublished> message)
    {
        Id = message.Data.VideoId;
        PublishingAttempted = true;
        OwnerId = message.Data.OwnerId;
        SourceUri = message.Data.SourceUri;
        TranscodedUri = message.Data.TranscodedUri;
    }

    public void VideoPublishingFailed(Message<VideoPublishingFailed> message)
    {
        Id = message.Data.VideoId;
        PublishingAttempted = true;
        OwnerId = message.Data.OwnerId;
        SourceUri = message.Data.SourceUri;
    }
}