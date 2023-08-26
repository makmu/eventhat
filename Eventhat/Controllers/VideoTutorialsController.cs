using Eventhat.Events;
using Eventhat.InfraStructure;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("[controller]")]
public class VideoTutorialsController : ControllerBase
{
    private readonly ILogger<VideoTutorialsController> _logger;
    private readonly MessageStore _messageStore;

    public VideoTutorialsController(
        ILogger<VideoTutorialsController> logger,
        MessageStore messageStore)
    {
        _logger = logger;
        _messageStore = messageStore;
    }

    [HttpPost(Name = "recordViewing")]
    public async Task<ActionResult> RecordViewingAsync([FromBody] Guid videoId)
    {
        var traceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var viewedEvent = new Message<VideoViewed>(Guid.NewGuid(), new Metadata(traceId, userId), new VideoViewed(userId, videoId));

        // TODO: get expected version
        var expectedVersion = 0;
        await _messageStore.WriteAsync("viewing", viewedEvent, expectedVersion);

        return Accepted();
    }
}