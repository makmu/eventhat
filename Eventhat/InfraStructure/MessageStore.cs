using System.Text.Json;
using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class MessageStore
{
    private readonly IMessageStreamDatabase _database;

    public MessageStore(IMessageStreamDatabase database)
    {
        _database = database;
    }

    public async Task WriteAsync<T>(string streamName, Message<T> message, int expectedVersion)
    {
        await _database.WriteAsync(message.Id, streamName, typeof(T).ToString(), JsonSerializer.Serialize(message.Data), JsonSerializer.Serialize(message.Meta), expectedVersion);
    }
}