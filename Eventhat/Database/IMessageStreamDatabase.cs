namespace Eventhat.Database;

public interface IMessageStreamDatabase
{
    public IQueryable<Page> Query { get; }
    public Task WriteMessageAsync(Guid id, string streamName, string type, string data, string metadata, int? expectedVersion);
    public Task<IEnumerable<MessageEntity>> GetCategoryMessages(string streamName, int fromPosition, int batchSize);

    public Task<MessageEntity?> GetLastStreamMessage(string streamName);
    Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition, int batchSize);

    public Task AddAsync(Page page);

    public Task UpdateAsync(Page page);
}