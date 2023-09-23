namespace Eventhat.Controllers;

public class UserAttributesDto
{
    public Guid Id { get; }
    public string Email { get; }
    public string Password { get; }

    public UserAttributesDto(Guid id, string email, string password)
    {
        Id = id;
        Email = email;
        Password = password;
    }
}