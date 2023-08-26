namespace Eventhat.Database;

public interface IMessageStreamDatabase
{
    public Task WriteAsync(Guid id, string streamName, string type, string data, string metadata, int expectedVersion);
}