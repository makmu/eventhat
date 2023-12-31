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
    private readonly MessageStore _messageStore;
    private readonly ViewDataContext _viewData;

    public CreatorsPortalController(MessageStore messageStore, ViewDataContext viewData)
    {
        _messageStore = messageStore;
        _viewData = viewData;
    }

    [HttpPost("publish-video")]
    public async Task<ActionResult> PublishVideoAsync([FromBody] VideoPublishingDto videoPublishing)
    {
        var traceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var publishVideo = new PublishVideo(videoPublishing.VideoId, videoPublishing.OwnerId, videoPublishing.SourceUri);

        await _messageStore.WriteAsync($"videoPublishing:command-{videoPublishing.VideoId}", new Metadata(traceId, userId), publishVideo, 0);

        return Accepted(traceId);
    }

    [HttpPost("name-video/{videoId}")]
    public async Task<ActionResult> NameVideoAsync(Guid videoId, [FromBody] string name)
    {
        var traceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var publishVideo = new NameVideo(videoId, name);

        await _messageStore.WriteAsync($"videoPublishing:command-{videoId}", new Metadata(traceId, userId), publishVideo);

        return Accepted(traceId);
    }

    [HttpGet("video-operations/{traceId}")]
    public Task<ActionResult<VideoOperationDto>> GetVideoOperationByTraceIdAsync(Guid traceId)
    {
        return Task.FromResult<ActionResult<VideoOperationDto>>(
            Ok(
                _viewData.VideoOperations
                    .Where(x => x.TraceId == traceId)
                    .Select(x => new VideoOperationDto(x.TraceId, x.VideoId, x.Succeeded, x.FailureReason))
                    .FirstOrDefault()));
    }

    [HttpGet("video/{videoId}")]
    public Task<ActionResult<VideoDto>> GetVideoById(Guid videoId)
    {
        return Task.FromResult<ActionResult<VideoDto>>(
            Ok(
                _viewData.CreatorVideos
                    .Where(x => x.Id == videoId)
                    .Select(x => new VideoDto(x.Id, x.Name, x.TranscodedUri))
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
        public VideoDto(Guid videoId, string name, Uri transcodedUri)
        {
            VideoId = videoId;
            Name = name;
            TranscodedUri = transcodedUri;
        }

        public Guid VideoId { get; }
        public string Name { get; }
        public Uri TranscodedUri { get; }
    }
}