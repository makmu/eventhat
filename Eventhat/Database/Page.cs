namespace Eventhat.Database;

public class Page
{
    public Page(string name, string data)
    {
        Name = name;
        Data = data;
    }

    public string Name { get; }
    public string Data { get; }
}