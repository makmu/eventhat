namespace Eventhat.Aggregators;

public class HomepageData
{
    public HomepageData(int videosWatched, int lastViewProcessed)
    {
        VideosWatched = videosWatched;
        LastViewProcessed = lastViewProcessed;
    }

    public int VideosWatched { get; }
    public int LastViewProcessed { get; }
}