using Eventhat.Database;

namespace Eventhat.Testing;

public class InMemoryMessageStreamDatabase : IMessageStreamDatabase
{
    private readonly List<MessageEntity> _messagesTable = new();

    public Task WriteAsync(Guid id, string streamName, string type, string data, string metadata, int expectedVersion)
    {
        var streamVersion = _messagesTable.Count(m => m.StreamName == streamName);
        if (streamVersion != expectedVersion) throw new Exception($"Version Conflict: stream {streamName}, streamVersion {streamVersion}, expectedVersion {expectedVersion} ");

        _messagesTable.Add(new MessageEntity(id, streamName, type, streamVersion, _messagesTable.Count, data, metadata, DateTimeOffset.Now));

        return Task.CompletedTask;
    }
}