using Eventhat.Helpers;
using Eventhat.InfraStructure;

namespace Eventhat.Projections;

public abstract class ProjectionBase
{
    private readonly Dictionary<Type, Action<Guid, string, string, string, int, int>> _handlers = new();

    protected void RegisterEventHandler<T>(Action<Message<T>> handler)
    {
        _handlers.Add(typeof(T),
            (id, streamName, metadata, data, position, globalPosition) => handler(new Message<T>(id, streamName, metadata.Deserialize<Metadata>(), data.Deserialize<T>(), position, globalPosition)));
    }

    public void ApplyEvent(string messageType, Guid id, string streamName, string metadata, string data, int position, int globalPosition)
    {
        var type = Type.GetType(messageType);
        if (type == null) throw new Exception($"Unknown message type {messageType}");
        if (_handlers.TryGetValue(type, out var handler)) handler(id, streamName, metadata, data, position, globalPosition);
    }
}