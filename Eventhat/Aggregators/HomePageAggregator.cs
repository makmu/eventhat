using System.Text.Json;
using Eventhat.Database;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Aggregators;

public class HomePageAggregator : IAgent
{
    private readonly MessageSubscription _subscription;
    private readonly IDbContextFactory<ViewDataContext> _viewDataDb;

    public HomePageAggregator(IDbContextFactory<ViewDataContext> viewDataContextDb, MessageStore messageStore)
    {
        _viewDataDb = viewDataContextDb;
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

    public Task VideoViewedAsync(Message<VideoViewed> message)
    {
        using var viewData = _viewDataDb.CreateDbContext();
        var page = viewData.Pages.Single(p => p.Name == "home");
        var storedHomepageData = JsonSerializer.Deserialize<HomepageData>(page.Data);
        if (storedHomepageData == null) throw new Exception("Cannot deserialize homepage data");

        if (storedHomepageData.LastViewProcessed >= message.GlobalPosition) return Task.CompletedTask;

        var updatedHomepageData = new HomepageData(storedHomepageData.VideosWatched + 1, message.GlobalPosition);

        page.Data = JsonSerializer.Serialize(updatedHomepageData);
        return Task.CompletedTask;
    }

    public async Task EnsureHomepage()
    {
        using var viewData = _viewDataDb.CreateDbContext();
        if (viewData.Pages.All(x => x.Name != "home"))
        {
            await viewData.AddAsync(
                new Page
                {
                    Name = "home",
                    Data = JsonSerializer.Serialize(new HomepageData(0, 0))
                });

            await viewData.SaveChangesAsync();
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