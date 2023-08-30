namespace Eventhat.Helpers;

public static class StreamNameExtensions
{
    public static string GetCategory(this string streamName)
    {
        if (string.IsNullOrEmpty(streamName)) return string.Empty;

        var categoryName = streamName.Split('-').First();
        return string.IsNullOrEmpty(categoryName) ? string.Empty : categoryName;
    }

    public static Guid ToId(this string streamName)
    {
        return new Guid(streamName.Substring(streamName.IndexOf('-') + 1));
    }
}