using System.ComponentModel.DataAnnotations;

namespace Eventhat.Database.Entities;

public class VideoOperation
{
    [Key]
    public Guid TraceId { get; set; }

    public Guid VideoId { get; set; }
    public bool Succeeded { get; set; }
    public string? FailureReason { get; set; }
}