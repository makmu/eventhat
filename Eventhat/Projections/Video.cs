using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.Messages.Events;

namespace Eventhat.Projections;

public class Video
{
    public Guid Id { get; set; }
    public bool PublishingAttempted { get; set; }
    public Uri? SourceUri { get; set; }
    public Uri? TranscodedUri { get; set; }

    public Guid OwnerId { get; set; }

    public class Projection
    {
        public Dictionary<Type, Func<Video, MessageEntity, Video>> AsDictionary()
        {
            return new Dictionary<Type, Func<Video, MessageEntity, Video>>
            {
                { typeof(VideoPublished), VideoPublished },
                { typeof(VideoPublishingFailed), VideoPublishingFailed }
            };
        }

        private Video VideoPublished(Video video, MessageEntity message)
        {
            var data = message.Data.Deserialize<VideoPublished>();
            video.Id = data.VideoId;
            video.PublishingAttempted = true;
            video.OwnerId = data.OwnerId;
            video.SourceUri = data.SourceUri;
            video.TranscodedUri = data.TranscodedUri;
            return video;
        }

        public Video VideoPublishingFailed(Video video, MessageEntity message)
        {
            var data = message.Data.Deserialize<VideoPublishingFailed>();
            video.Id = data.VideoId;
            video.PublishingAttempted = true;
            video.OwnerId = data.OwnerId;
            video.SourceUri = data.SourceUri;

            return video;
        }
    }
}