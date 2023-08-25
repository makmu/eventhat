namespace Eventhat.InfraStructure;

public class MessageStoryDummy : IMessageStore
{
    public Task WriteAsync(string category, Guid entityId, object evt)
    {
        return Task.CompletedTask;
    }
}