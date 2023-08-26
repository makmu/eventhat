namespace Eventhat.Controllers.Dto;

public class PageDto
{
    public PageDto(string data)
    {
        Data = data;
    }

    public string Data { get; }
}