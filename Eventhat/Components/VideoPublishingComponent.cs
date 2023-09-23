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
        _subscription.RegisterHandler<NameVideo>(NameVideoAsync);
    }

    public void Start()
    {
        _ = _subscription.StartAsync();
    }

    public void Stop()
    {
        _subscription.Stop();
    }

    private async Task NameVideoAsync(Message<NameVideo> command)
    {
        try
        {
            var video = await LoadVideoAsync(command.Data.VideoId);
            EnsureCommandHasNotBeenProcessed(command.GlobalPosition, video);
            EnsureNameIsValid(command.Data.Name);
            await WriteVideoNamedEventAsync(command);
        }
        catch (CommandAlreadyProcessedException)
        {
            // to nothing
        }
        catch (NameValidationException e)
        {
            await WriteVideoNameRejectedEventAsync(command, e);
        }
    }

    private async Task WriteVideoNameRejectedEventAsync(Message<NameVideo> command, NameValidationException e)
    {
        await _messageStore.WriteAsync(
            $"videoPublishing-{command.Data.VideoId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId),
            new VideoNameRejected(command.Data.Name, e.Message));
    }

    private async Task WriteVideoNamedEventAsync(Message<NameVideo> command)
    {
        await _messageStore.WriteAsync(
            $"videoPublishing-{command.Data.VideoId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId),
            new VideoNamed(command.Data.Name));
    }

    private void EnsureNameIsValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new NameValidationException("Video name must not be empty");
    }

    private void EnsureCommandHasNotBeenProcessed(int globalPosition, Video video)
    {
        if (video.Sequence > globalPosition)
            throw new CommandAlreadyProcessedException();
    }

    private async Task PublishVideoAsync(Message<PublishVideo> command)
    {
        try
        {
            var video = await LoadVideoAsync(command.Data.VideoId);
            EnsurePublishingNotAttempted(video);
            var transcodedUri = await TranscodeVideoAsync();
            await WriteVideoPublishedEvent(command, transcodedUri);
        }
        catch (AlreadyPublishedException)
        {
            // to nothing
        }
        catch (Exception e)
        {
            await WriteVideoPublishingFailedEvent(command, e);
        }
    }

    private async Task WriteVideoPublishingFailedEvent(Message<PublishVideo> command, Exception exception)
    {
        await _messageStore.WriteAsync(
            $"videoPublishing-{command.Data.VideoId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId, command.Metadata.OriginStreamName),
            new VideoPublishingFailed(command.Data.VideoId, command.Data.OwnerId, command.Data.SourceUri, exception.Message));
    }

    private async Task WriteVideoPublishedEvent(Message<PublishVideo> command, Uri transcodedUri)
    {
        await _messageStore.WriteAsync(
            $"videoPublishing-{command.Data.VideoId}",
            new Metadata(command.Metadata.TraceId, command.Metadata.UserId),
            new VideoPublished(command.Data.VideoId, command.Data.OwnerId, command.Data.SourceUri, transcodedUri));
    }

    private async Task<Uri> TranscodeVideoAsync()
    {
        Console.WriteLine("Transcoding Video...");
        await Task.Delay(3000);
        Console.WriteLine("Done...");

        return new Uri("http://somewhere.over.the.rainbow.com/file.mkv");
    }

    private void EnsurePublishingNotAttempted(Video video)
    {
        if (video.PublishingAttempted) throw new AlreadyPublishedException();
    }

    private async Task<Video> LoadVideoAsync(Guid videoId)
    {
        return await _messageStore.FetchAsync<Video>($"videoPublishing-{videoId}");
    }
}