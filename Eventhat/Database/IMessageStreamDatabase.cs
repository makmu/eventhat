using Eventhat.Database.Entities;

namespace Eventhat.Database;

public interface IMessageStreamDatabase
{
    public IQueryable<Page> Pages { get; }
    public IQueryable<UserCredentials> UserCredentials { get; }
    public Task WriteMessageAsync(Guid id, string streamName, string type, string data, string metadata, int? expectedVersion);
    public Task<IEnumerable<MessageEntity>> GetCategoryMessages(string streamName, int fromPosition, int batchSize);

    public Task<MessageEntity?> GetLastStreamMessage(string streamName);
    Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition, int batchSize);

    public Task InsertPageAsync(string name, string data);

    public Task UpdatePageAsync(string name, string data);

    public Task InsertUserCredentialAsync(Guid id, string email, string passwordHash);
}