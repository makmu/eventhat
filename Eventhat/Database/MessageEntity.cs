using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventhat.Database;

public class MessageEntity
{
    public Guid Id { get; set; }
    public string StreamName { get; set; }
    public string Type { get; set; }
    public int Position { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int GlobalPosition { get; set; }

    public string Data { get; set; }
    public string Metadata { get; set; }
    public DateTimeOffset Time { get; set; }
}