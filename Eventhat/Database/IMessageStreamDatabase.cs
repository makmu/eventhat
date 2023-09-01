using Eventhat.Database.Entities;

namespace Eventhat.Database;

public interface IMessageStreamDatabase
{
    IQueryable<Page> Pages { get; }
    IQueryable<UserCredentials> UserCredentials { get; }
    IQueryable<VideoOperation> VideoOperations { get; }
    IQueryable<CreatorVideo> CreatorVideos { get; }
    Task WriteMessageAsync(Guid id, string streamName, string type, string data, string metadata, int? expectedVersion);
    Task<IEnumerable<MessageEntity>> GetCategoryMessages(string streamName, int fromPosition, int batchSize);

    Task<MessageEntity?> GetLastStreamMessage(string streamName);
    Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition, int batchSize);

    Task InsertPageAsync(string name, string data);

    Task UpdatePageAsync(string name, string data);

    Task InsertUserCredentialAsync(Guid id, string email, string passwordHash);

    Task InsertVideoOperationAsync(Guid traceId, Guid videoId, bool succeeded, string reason);
    Task InsertCreatorsVideoAsync(Guid videoId, Uri transcodedUri, int position);
    Task UpdateCreatorsVideoAsync(Guid videoId, string name, int position);
}