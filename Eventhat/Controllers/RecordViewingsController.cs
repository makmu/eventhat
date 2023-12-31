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

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        // TODO: get expected version
        var expectedVersion = 0;
        await _messageStore.WriteAsync(
            $"viewing-{videoId}",
            new Metadata(traceId, userId),
            new VideoViewed(userId, videoId),
            expectedVersion);

        return Accepted(traceId);
    }
}