using System.ComponentModel.DataAnnotations;

namespace Eventhat.Database.Entities;

public class AdminStream
{
    [Key]
    public string StreamName { get; set; }

    public int MessageCount { get; set; }
    public Guid LastMessageId { get; set; }
    public int LastMessageGlobalPosition { get; set; }
}