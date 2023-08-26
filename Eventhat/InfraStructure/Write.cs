using System.Text.Json;
using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class Write
{
    private readonly IMessageStreamDatabase _database;

    public Write(IMessageStreamDatabase database)
    {
        _database = database;
    }

    public async Task WriteAsync<T>(string streamName, Message<T> message, int? expectedVersion = null)
    {
        await _database.WriteMessageAsync(message.Id, streamName, typeof(T).ToString(), JsonSerializer.Serialize(message.Data), JsonSerializer.Serialize(message.Meta), expectedVersion);
    }
}