using System.ComponentModel.DataAnnotations;

namespace Eventhat.Database;

public class Page
{
    [Key]
    public string Name { get; set; }

    public string Data { get; set; }
}