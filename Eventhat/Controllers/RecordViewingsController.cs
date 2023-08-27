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
        var tranceId = Guid.NewGuid();

        // TODO: get user id from request
        var userId = Guid.NewGuid();

        var viewedEvent = new Message<VideoViewed>(Guid.NewGuid(), new Metadata(tranceId, userId), new VideoViewed(userId, videoId));

        // TODO: get expected version
        var expectedVersion = 0;
        await _messageStore.WriteAsync($"viewing-{videoId}", viewedEvent, expectedVersion);

        return Accepted();
    }
}