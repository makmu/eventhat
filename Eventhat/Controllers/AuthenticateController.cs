using Eventhat.Controllers.Exceptions;
using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Eventhat.Messages.Events;
using Eventhat.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly IMessageStreamDatabase _db;
    private readonly MessageStore _messageStore;

    public AuthenticateController(
        IMessageStreamDatabase db,
        MessageStore messageStore)
    {
        _db = db;
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
            return Ok();
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

    private async Task HandleCredentialMismatchAsync(Guid traceId, Guid userId)
    {
        var userLoginFailedEvent = new Message<UserLoginFailed>(Guid.NewGuid(), new Metadata(traceId, userId), new UserLoginFailed(userId, "Incorrect password"));
        var streamName = $"authentication-{userId}";
        await _messageStore.WriteAsync(streamName, userLoginFailedEvent);
    }

    private async Task WriteLoggedInEventAsync(Guid traceId, Guid userId)
    {
        var userLoggedInEvent = new Message<UserLoggedIn>(Guid.NewGuid(), new Metadata(traceId, userId), new UserLoggedIn(userId));
        var streamName = $"authentication-{userId}";
        await _messageStore.WriteAsync(streamName, userLoggedInEvent);
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
        return await _db.UserCredentials.ByEmailAsync(email);
    }
}