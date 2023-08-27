using System.Text.Json;
using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;

namespace Eventhat.Aggregators;

public class HomePageAggregator : IAgent
{
    private readonly Queries _queries;
    private readonly MessageSubscription _subscription;

    public HomePageAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        _queries = new Queries(db);
        var handlers = new Handlers(_queries);
        _subscription = messageStore.CreateSubscription(
            "viewing",
            handlers.AsDictionary(),
            "aggregators:home-page");
    }

    public async Task StartAsync()
    {
        await InitAsync();
        await _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    public async Task InitAsync()
    {
        await _queries.EnsureHomepage();
    }

    public class Handlers
    {
        private readonly Queries _queries;

        public Handlers(Queries queries)
        {
            _queries = queries;
        }

        public async Task VideoViewedAsync(MessageEntity message)
        {
            await _queries.IncrementVideosWatchedAsync(message.GlobalPosition);
        }

        public Dictionary<Type, Func<MessageEntity, Task>> AsDictionary()
        {
            return new Dictionary<Type, Func<MessageEntity, Task>> { { typeof(VideoViewed), VideoViewedAsync } };
        }
    }

    public class Queries
    {
        private readonly IMessageStreamDatabase _db;

        public Queries(IMessageStreamDatabase db)
        {
            _db = db;
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