using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventhat.Controllers.Exceptions;
using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Eventhat.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Eventhat.Controllers;

[ApiController]
[Route("/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly MessageStore _messageStore;
    private readonly ViewDataContext _viewData;

    public AuthenticateController(
        ViewDataContext viewData,
        MessageStore messageStore)
    {
        _viewData = viewData;
        _messageStore = messageStore;
    }

    [HttpPost]
    public async Task<ActionResult> AuthenticateAsync([FromBody] AuthenticateDto attributes)
    {
        var traceId = Guid.NewGuid();
        var userCredential = await LoadUserCredentialAsync(attributes.Email);
        try
        {
            EnsureUserCredentialFound(userCredential);
            ValidatePassword(userCredential!.Id, attributes.Password, userCredential.PasswordHash);
            await WriteLoggedInEventAsync(traceId, userCredential.Id);

            return Ok(GenerateJwtToken(userCredential.Id));
        }
        catch (NotFoundException)
        {
            return BadRequest("Authentication failed");
        }
        catch (CredentialMismatchException e)
        {
            await HandleCredentialMismatchAsync(traceId, e.UserId);
            return BadRequest("Authentication failed");
        }
    }

    private string GenerateJwtToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this is my custom Secret key for authentication");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, userId.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task HandleCredentialMismatchAsync(Guid traceId, Guid userId)
    {
        await _messageStore.WriteAsync(
            $"authentication-{userId}",
            new Metadata(traceId, userId),
            new UserLoginFailed(userId, "Incorrect password"));
    }

    private async Task WriteLoggedInEventAsync(Guid traceId, Guid userId)
    {
        await _messageStore.WriteAsync(
            $"authentication-{userId}",
            new Metadata(traceId, userId),
            new UserLoggedIn(userId));
    }

    private void ValidatePassword(Guid userId, string providedPassword, string storedPasswordHash)
    {
        if (!BCrypt.Net.BCrypt.Verify(providedPassword, storedPasswordHash)) throw new CredentialMismatchException(userId);
    }

    private void EnsureUserCredentialFound(UserCredentials? userCredentials)
    {
        if (userCredentials == null) throw new NotFoundException();
    }

    private async Task<UserCredentials?> LoadUserCredentialAsync(string email)
    {
        return await _viewData.UserCredentials.ByEmailAsync(email);
    }
}