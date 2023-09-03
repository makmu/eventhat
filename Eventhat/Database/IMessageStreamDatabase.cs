using Eventhat.Database.Entities;

namespace Eventhat.Database;

public interface IMessageStreamDatabase
{
    IQueryable<Page> Pages { get; }
    IQueryable<UserCredentials> UserCredentials { get; }
    IQueryable<VideoOperation> VideoOperations { get; }
    IQueryable<CreatorVideo> CreatorVideos { get; }
    IQueryable<AdminUser> AdminUsers { get; }
    IQueryable<MessageEntity> Messages { get; }
    IQueryable<AdminStream> AdminStreams { get; }
    Task WriteMessageAsync(Guid id, string streamName, string type, string metadata, string data, int? expectedVersion);
    Task<IEnumerable<MessageEntity>> GetCategoryMessages(string streamName, int fromPosition, int batchSize);

    Task<MessageEntity?> GetLastStreamMessage(string streamName);
    Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition, int batchSize);

    Task InsertPageAsync(string name, string data);

    Task UpdatePageAsync(string name, string data);

    Task InsertUserCredentialAsync(Guid id, string email, string passwordHash);

    Task InsertVideoOperationAsync(Guid traceId, Guid videoId, bool succeeded, string reason);
    Task InsertCreatorsVideoAsync(Guid videoId, Uri transcodedUri, int position);
    Task UpdateCreatorsVideoAsync(Guid videoId, string name, int position);

    Task InsertAdminUsersAsync(
        Guid id);

    Task SetAdminUserEmail(
        Guid id,
        string email,
        int lastIdentityEventGlobalPosition);

    Task MarkRegistrationEmailSent(Guid dataIdentityId, int globalPosition);
    Task IncreaseAdminUserLoginCount(Guid dataUserId, int globalPosition);
    Task UpsertStream(string streamName, Guid messageId, int globalPosition);
}