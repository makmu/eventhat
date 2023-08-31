using Eventhat.InfraStructure;
using Eventhat.Messages.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/publish-video")]
public class VideoPublishingController : ControllerBase
{
    private readonly MessageStore _messageStore;

    public VideoPublishingController(MessageStore messageStore
    )
    {
        _messageStore = messageStore;
    }

    [HttpPost]
    public async Task<ActionResult> PublishVideoAsync([FromBody] VideoPublishingDto videoPublishing)
    {
        var tranceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var publishVideo = new Message<PublishVideo>(Guid.NewGuid(), new Metadata(tranceId, userId), new PublishVideo(videoPublishing.VideoId, videoPublishing.OwnerId, videoPublishing.SourceUri));

        await _messageStore.WriteAsync($"videoPublishing:command-{videoPublishing.VideoId}", publishVideo, 0);

        return Accepted();
    }

    public class VideoPublishingDto
    {
        public VideoPublishingDto(Guid videoId, Guid ownerId, Uri sourceUri)
        {
            VideoId = videoId;
            OwnerId = ownerId;
            SourceUri = sourceUri;
        }

        public Guid VideoId { get; }
        public Guid OwnerId { get; }
        public Uri SourceUri { get; }
    }
}