namespace Eventhat.Database.Entities;

public class CreatorVideo
{
    public Guid Id { get; set; }
    public Uri TranscodedUri { get; set; }
    public string Name { get; set; }
    public int Position { get; set; }
}