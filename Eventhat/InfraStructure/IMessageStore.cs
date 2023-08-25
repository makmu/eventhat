namespace Eventhat.InfraStructure;

public interface IMessageStore
{
    Task WriteAsync(string category, Guid entityId, object evt);
}