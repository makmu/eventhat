using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/record-viewing")]
public class RecordViewingsController : ControllerBase
{
    private readonly MessageStore _messageStore;

    public RecordViewingsController(MessageStore messageStore
    )
    {
        _messageStore = messageStore;
    }

    [HttpPost]
    public async Task<ActionResult> RecordViewingAsync([FromBody] Guid videoId)
    {
        var traceId = Guid.NewGuid();

        var userId = User.Id();
        if (userId == null) return BadRequest("Missing user id in authentication");

        await _messageStore.WriteAsync(
            $"viewing-{videoId}",
            new Metadata(traceId, userId.Value),
            new VideoViewed(userId.Value, videoId));

        return Accepted(traceId);
    }
}