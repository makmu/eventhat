namespace Eventhat.Database.Entities;

public class CreatorVideo
{
    public CreatorVideo(Guid videoId, Uri transcodedUri, string name, int position)
    {
        VideoId = videoId;
        TranscodedUri = transcodedUri;
        Name = name;
        Position = position;
    }

    public Guid VideoId { get; }
    public Uri TranscodedUri { get; }
    public string Name { get; }
    public int Position { get; }
}