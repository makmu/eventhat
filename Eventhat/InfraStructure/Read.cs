using Eventhat.Database;

namespace Eventhat.InfraStructure;

public class Read
{
    private readonly IMessageStreamDatabase _database;

    public Read(IMessageStreamDatabase database)
    {
        _database = database;
    }

    public async Task<IEnumerable<MessageEntity>> ReadAsync(string streamName, int fromPosition, int messagesPerTick)
    {
        if (streamName.Contains('-'))
            return await _database.GetStreamMessages(streamName, fromPosition, messagesPerTick);

        return await _database.GetCategoryMessages(streamName, fromPosition, messagesPerTick);
    }

    public async Task<MessageEntity?> ReadLastMessageAsync(string streamName)
    {
        return await _database.GetLastStreamMessage(streamName);
    }
}