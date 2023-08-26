using Eventhat.Events;
using Eventhat.InfraStructure;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/")]
public class RecordViewingsController : ControllerBase
{
    private readonly ILogger<RecordViewingsController> _logger;
    private readonly MessageStore _messageStore;

    public RecordViewingsController(
        ILogger<RecordViewingsController> logger,
        MessageStore messageStore
    )
    {
        _logger = logger;
        _messageStore = messageStore;
    }

    [HttpPost("recordViewing")]
    public async Task<ActionResult> RecordViewingAsync([FromBody] Guid videoId)
    {
        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var viewedEvent = new Message<VideoViewed>(Guid.NewGuid(), new Metadata(Guid.NewGuid(), userId), new VideoViewed(userId, videoId));

        // TODO: get expected version
        var expectedVersion = 0;
        await _messageStore.Write.WriteAsync($"viewing-{videoId}", viewedEvent, expectedVersion);

        return Accepted();
    }
}