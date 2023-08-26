using Eventhat.Database;
using Eventhat.Helpers;

namespace Eventhat.Testing;

public class InMemoryMessageStreamDatabase : IMessageStreamDatabase
{
    private readonly List<MessageEntity> _messagesTable = new();

    private List<Page> _entities = new();

    public Task WriteMessageAsync(Guid id, string streamName, string type, string data, string metadata, int? expectedVersion)
    {
        var streamVersion = _messagesTable.Count(m => m.StreamName == streamName);
        if (expectedVersion != null && streamVersion != expectedVersion)
            throw new Exception($"Version Conflict: stream {streamName}, streamVersion {streamVersion}, expectedVersion {expectedVersion} ");

        _messagesTable.Add(new MessageEntity(id, streamName, type, streamVersion + 1, _messagesTable.Count, data, metadata, DateTimeOffset.Now));

        return Task.CompletedTask;
    }

    public Task<IEnumerable<MessageEntity>> GetCategoryMessages(string categoryName, int fromPosition = 0, int batchSize = 1000)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName.GetCategory() == categoryName && m.GlobalPosition >= fromPosition).OrderBy(m => m.GlobalPosition).Take(batchSize));
    }

    public Task<MessageEntity?> GetLastStreamMessage(string streamName)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName == streamName).MaxBy(m => m.Position));
    }

    public Task<IEnumerable<MessageEntity>> GetStreamMessages(string streamName, int fromPosition = 0, int batchSize = 1000)
    {
        return Task.FromResult(_messagesTable.Where(m => m.StreamName == streamName && m.Position >= fromPosition).OrderBy(m => m.Position).Take(batchSize));
    }

    public IQueryable<Page> Query => _entities.AsQueryable();

    public Task AddAsync(Page page)
    {
        _entities.Add(page);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Page page)
    {
        _entities = _entities.Where(x => x.Name != page.Name).ToList();
        _entities.Add(page);
        return Task.CompletedTask;
    }
}