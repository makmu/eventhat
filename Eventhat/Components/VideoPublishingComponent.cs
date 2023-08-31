using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Eventhat.Messages.Commands;
using Eventhat.Messages.Events;
using Eventhat.Projections;

namespace Eventhat.Components;

public class VideoPublishingComponent : IAgent
{
    private readonly MessageStore _messageStore;
    private readonly MessageSubscription _subscription;

    public VideoPublishingComponent(MessageStore messageStore)
    {
        _messageStore = messageStore;
        _subscription = messageStore.CreateSubscription(
            "videoPublishing:command",
            "components:video-publishing"
        );
        _subscription.RegisterHandler<PublishVideo>(PublishVideoAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task PublishVideoAsync(MessageEntity command)
    {
        try
        {
            var video = await LoadVideoAsync(command);
            EnsurePublishingNotAttempted(video);
            var transcodedUri = await TranscodeVideoAsync(command);
            await WriteVideoPublishedEvent(command, transcodedUri);
        }
        catch (AlreadyPublishedException e)
        {
            // to nothing
        }
        catch (Exception e)
        {
            await WriteVideoPublishingFailedEvent(command, e);
        }
    }

    private async Task WriteVideoPublishingFailedEvent(MessageEntity command, Exception exception)
    {
        var data = command.Data.Deserialize<PublishVideo>();
        var metadata = command.Metadata.Deserialize<Metadata>();

        var streamName = $"videoPublishing-{data.VideoId}";

        await _messageStore.WriteAsync(streamName,
            new Message<VideoPublishingFailed>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId, metadata.OriginStreamName),
                new VideoPublishingFailed(data.VideoId, data.OwnerId, data.SourceUri, exception.Message)));
    }

    private async Task WriteVideoPublishedEvent(MessageEntity command, Uri transcodedUri)
    {
        var data = command.Data.Deserialize<PublishVideo>();
        var metadata = command.Metadata.Deserialize<Metadata>();

        var streamName = $"videoPublishing-{data.VideoId}";

        await _messageStore.WriteAsync(streamName,
            new Message<VideoPublished>(Guid.NewGuid(), new Metadata(metadata.TraceId, metadata.UserId), new VideoPublished(data.VideoId, data.OwnerId, data.SourceUri, transcodedUri)));
    }

    private Task<Uri> TranscodeVideoAsync(MessageEntity publishCommand)
    {
        var data = publishCommand.Data.Deserialize<PublishVideo>();
        Console.WriteLine("Transcoding Video...");
        var videoFakeData = Enumerable.Range(0, 10000000).Select(x => Guid.NewGuid()).ToList();
        videoFakeData.Sort();
        Console.WriteLine("Done...");

        return Task.FromResult(new Uri("http://somewhere.over.the.rainbow.com/file.mkv"));
    }

    private void EnsurePublishingNotAttempted(Video video)
    {
        if (video.PublishingAttempted) throw new AlreadyPublishedException();
    }

    private async Task<Video> LoadVideoAsync(MessageEntity messageEntity)
    {
        var data = messageEntity.Data.Deserialize<PublishVideo>();
        var videoProjection = new Video.Projection();
        return await _messageStore.FetchAsync($"videoPublishing-{data.VideoId}", videoProjection.AsDictionary());
    }
}