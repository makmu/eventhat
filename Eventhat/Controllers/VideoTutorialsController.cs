using Eventhat.Events;
using Eventhat.InfraStructure;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("[controller]")]
public class VideoTutorialsController : ControllerBase
{
    private readonly ILogger<VideoTutorialsController> _logger;
    private readonly IMessageStore _messageStore;

    public VideoTutorialsController(
        ILogger<VideoTutorialsController> logger,
        IMessageStore messageStore)
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

        var evt = new VideoViewed(traceId, userId, videoId);

        await _messageStore.WriteAsync("viewing", videoId, evt);

        return Accepted();
    }
}