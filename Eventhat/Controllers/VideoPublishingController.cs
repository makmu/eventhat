using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Commands;
using Eventhat.Messages.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("creators-portal")]
public class CreatorsPortalController : ControllerBase
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageStore _messageStore;

    public CreatorsPortalController(MessageStore messageStore, IMessageStreamDatabase db)
    {
        _messageStore = messageStore;
        _db = db;
    }

    [HttpPost("publish-video")]
    public async Task<ActionResult> PublishVideoAsync([FromBody] VideoPublishingDto videoPublishing)
    {
        var tranceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var publishVideo = new PublishVideo(videoPublishing.VideoId, videoPublishing.OwnerId, videoPublishing.SourceUri);

        await _messageStore.WriteAsync($"videoPublishing:command-{videoPublishing.VideoId}", new Metadata(tranceId, userId), publishVideo, 0);

        return Accepted();
    }

    [HttpPost("name-video/{videoId}")]
    public async Task<ActionResult> NameVideoAsync(Guid videoId, [FromBody] string name)
    {
        var tranceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var publishVideo = new NameVideo(videoId, name);

        await _messageStore.WriteAsync($"videoPublishing:command-{videoId}", new Metadata(tranceId, userId), publishVideo);

        return Accepted(tranceId);
    }

    [HttpGet("video-operations/{traceId}")]
    public Task<ActionResult<VideoOperationDto>> GetVideoOperationByTraceIdAsync(Guid traceId)
    {
        return Task.FromResult<ActionResult<VideoOperationDto>>(
            Ok(
                _db.VideoOperations
                    .Where(x => x.TraceId == traceId)
                    .Select(x => new VideoOperationDto(x.TraceId, x.VideoId, x.Succeeded, x.FailureReason))
                    .FirstOrDefault()));
    }

    [HttpGet("video/{videoId}")]
    public Task<ActionResult<VideoDto>> GetVideoById(Guid videoId)
    {
        return Task.FromResult<ActionResult<VideoDto>>(
            Ok(
                _db.CreatorVideos
                    .Where(x => x.VideoId == videoId)
                    .Select(x => new VideoDto(x.VideoId, x.Name, x.TranscodedUri))
                    .FirstOrDefault()));
    }

    public class VideoOperationDto
    {
        public VideoOperationDto(Guid traceId, Guid videoId, bool succeeded, string failureReason)
        {
            TraceId = traceId;
            VideoId = videoId;
            Succeeded = succeeded;
            FailureReason = failureReason;
        }

        public Guid TraceId { get; }
        public Guid VideoId { get; }
        public bool Succeeded { get; }
        public string FailureReason { get; }
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

    public class VideoDto
    {
        public VideoDto(Guid viceoId, string name, Uri transcodedUri)
        {
            ViceoId = viceoId;
            Name = name;
            TranscodedUri = transcodedUri;
        }

        public Guid ViceoId { get; }
        public string Name { get; }
        public Uri TranscodedUri { get; }
    }
}