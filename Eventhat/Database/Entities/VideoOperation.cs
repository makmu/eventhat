namespace Eventhat.Database.Entities;

public class VideoOperation
{
    public VideoOperation(Guid traceId, Guid videoId, bool succeeded, string failureReason)
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