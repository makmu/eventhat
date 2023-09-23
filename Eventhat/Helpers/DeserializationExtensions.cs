using System.Text.Json;

namespace Eventhat.Helpers;

public static class DeserializationExtensions
{
    public static T Deserialize<T>(this string str)
    {
        var deserialized = JsonSerializer.Deserialize<T>(str);
        if (deserialized == null) throw new Exception($"Could not deserialize type '{typeof(T)}' from '{str}'");

        return deserialized;
    }
}