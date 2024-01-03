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
    private readonly IConfiguration _configuration;
    private readonly MessageStore _messageStore;
    private readonly ViewDataContext _viewData;

    public AuthenticateController(
        ViewDataContext viewData,
        MessageStore messageStore,
        IConfiguration configuration)
    {
        _viewData = viewData;
        _messageStore = messageStore;
        _configuration = configuration;
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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authorization:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["Authorization:Issuer"],
            _configuration["Authorization:Audience"],
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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