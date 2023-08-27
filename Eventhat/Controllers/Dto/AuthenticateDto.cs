namespace Eventhat.Controllers;

public class AuthenticateDto
{
    public AuthenticateDto(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }
    public string Password { get; }
}