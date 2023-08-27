using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.Helpers;

namespace Eventhat.Testing;

public class InMemoryMessageStreamDatabase : IMessageStreamDatabase
{
    private readonly List<MessageEntity> _messagesTable = new();
    private readonly List<UserCredentials> _userCredentials = new();

    private List<Page> _pages = new();

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

    public IQueryable<Page> Pages => _pages.AsQueryable();
    public IQueryable<UserCredentials> UserCredentials => _userCredentials.AsQueryable();

    public Task InsertPageAsync(string name, string data)
    {
        if (_pages.Any(p => p.Name == name)) return Task.CompletedTask;

        _pages.Add(new Page(name, data));
        return Task.CompletedTask;
    }

    public Task InsertUserCredentialAsync(Guid id, string email, string passwordHash)
    {
        if (_userCredentials.Any(c => c.Id == id)) return Task.CompletedTask;

        _userCredentials.Add(new UserCredentials(id, email, passwordHash));
        return Task.CompletedTask;
    }

    public Task UpdatePageAsync(string name, string data)
    {
        _pages = _pages.Where(x => x.Name != name).ToList();
        _pages.Add(new Page(name, data));
        return Task.CompletedTask;
    }
}