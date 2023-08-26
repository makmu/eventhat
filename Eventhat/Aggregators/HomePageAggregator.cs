using System.Text.Json;
using Eventhat.Database;
using Eventhat.Events;
using Eventhat.InfraStructure;

namespace Eventhat.Aggregators;

public class HomePageAggregator : IAgent
{
    private readonly MessageSubscription _subscription;

    public HomePageAggregator(IMessageStreamDatabase db, MessageStore messageStore)
    {
        Queries = new QueriesObj(db);
        Handlers = new HandlersObj(Queries);
        _subscription = messageStore.CreateSubscription(
            "viewing",
            Handlers.AsDictionary(),
            "aggregators:home-page");
    }

    public HandlersObj Handlers { get; }
    public QueriesObj Queries { get; }

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
        await Queries.EnsureHomepage();
    }

    public class HandlersObj
    {
        private readonly QueriesObj _queries;

        public HandlersObj(QueriesObj queries)
        {
            _queries = queries;
        }

        public async Task HandleVideoViewed(MessageEntity message)
        {
            await _queries.IncrementVideosWatchedAsync(message.GlobalPosition);
        }

        public Dictionary<string, Func<MessageEntity, Task>> AsDictionary()
        {
            return new Dictionary<string, Func<MessageEntity, Task>> { { typeof(VideoViewed).ToString(), HandleVideoViewed } };
        }
    }

    public class QueriesObj
    {
        private readonly IMessageStreamDatabase _db;

        public QueriesObj(IMessageStreamDatabase db)
        {
            _db = db;
        }

        public async Task EnsureHomepage()
        {
            await _db.AddAsync(new Page("home", JsonSerializer.Serialize(new HomepageData(0, 0))));
        }

        public async Task IncrementVideosWatchedAsync(int globalPosition)
        {
            var page = _db.Query.Single(p => p.Name == "home");
            var storedHomepageData = JsonSerializer.Deserialize<HomepageData>(page.Data);
            if (storedHomepageData == null) throw new Exception("Cannot deserialize homepage data");

            if (storedHomepageData.LastViewProcessed >= globalPosition) return;

            var updatedHomepageData = new HomepageData(storedHomepageData.VideosWatched + 1, globalPosition);

            await _db.UpdateAsync(new Page(page.Name, JsonSerializer.Serialize(updatedHomepageData)));
        }
    }
}