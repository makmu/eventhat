using System.Text.Json;
using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class HomePageAggregator : IAgent
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageSubscription _subscription;

    public HomePageAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _db = db;
        _subscription = messageStore.CreateSubscription(
            "viewing",
            "aggregators:home-page");
        _subscription.RegisterHandler<VideoViewed>(VideoViewedAsync);
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    public void Start()
    {
        var task = InitAsync();
        task.ContinueWith(async delegate { await _subscription.StartAsync(); });
    }

    public async Task InitAsync()
    {
        await EnsureHomepage();
    }

    public async Task VideoViewedAsync(Message<VideoViewed> message)
    {
        await IncrementVideosWatchedAsync(message.GlobalPosition);
    }

    public async Task EnsureHomepage()
    {
        await _db.InsertPageAsync("home", JsonSerializer.Serialize(new HomepageData(0, 0)));
    }

    public async Task IncrementVideosWatchedAsync(int globalPosition)
    {
        var page = _db.Pages.Single(p => p.Name == "home");
        var storedHomepageData = JsonSerializer.Deserialize<HomepageData>(page.Data);
        if (storedHomepageData == null) throw new Exception("Cannot deserialize homepage data");

        if (storedHomepageData.LastViewProcessed >= globalPosition) return;

        var updatedHomepageData = new HomepageData(storedHomepageData.VideosWatched + 1, globalPosition);

        await _db.UpdatePageAsync(page.Name, JsonSerializer.Serialize(updatedHomepageData));
    }

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
}