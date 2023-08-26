namespace Eventhat.Helpers;

public static class StreamNameExtensions
{
    public static string GetCategory(this string str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;

        var categoryName = str.Split('-').First();
        return string.IsNullOrEmpty(categoryName) ? string.Empty : categoryName;
    }
}