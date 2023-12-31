using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Eventhat.Database;
using Eventhat.Database.Entities;
using Eventhat.InfraStructure;
using Eventhat.Messages.Commands;
using Eventhat.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/register")]
public class RegisterUsersController : ControllerBase
{
    private readonly MessageStore _messageStore;
    private readonly ViewDataContext _viewData;

    public RegisterUsersController(
        ViewDataContext viewData,
        MessageStore messageStore)
    {
        _viewData = viewData;
        _messageStore = messageStore;
    }

    [HttpPost]
    public async Task<ActionResult> RegisterUserAsync([FromBody] UserAttributesDto attributes)
    {
        var traceId = Guid.NewGuid();
        try
        {
            await ValidateAsync(attributes);
            var identity = await LoadExistingIdentityAsync(attributes);
            EnsureThereWasNoExistingIdentity(identity);
            var passwordHash = HashPassword(attributes);
            await WriteRegisterCommandAsync(traceId, attributes.Id, attributes.Email, passwordHash);
            return Accepted(traceId);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public Task<ActionResult> RegistrationCompleteAsync()
    {
        return Task.FromResult<ActionResult>(BadRequest());
    }

    private async Task WriteRegisterCommandAsync(Guid traceId, Guid userId, string email, string passwordHash)
    {
        await _messageStore.WriteAsync(
            $"identity:command-{userId}",
            new Metadata(traceId, userId),
            new Register(userId, email, passwordHash));
    }

    private string HashPassword(UserAttributesDto attributes)
    {
        const int saltRounds = 10;

        return BCrypt.Net.BCrypt.HashPassword(attributes.Password, saltRounds);
    }

    private void EnsureThereWasNoExistingIdentity(UserCredentials? existingIdentity)
    {
        if (existingIdentity != null) throw new ValidationException("email already taken");
    }

    private async Task<UserCredentials?> LoadExistingIdentityAsync(UserAttributesDto attributes)
    {
        return await _viewData.UserCredentials.ByEmailAsync(attributes.Email);
    }

    private Task ValidateAsync(UserAttributesDto attributes)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(attributes.Email))
            errors.Add("Missing email address");
        else
            try
            {
                _ = new MailAddress(attributes.Email);
            }
            catch (FormatException e)
            {
                errors.Add(e.Message);
            }

        if (string.IsNullOrEmpty(attributes.Password))
            errors.Add("Missing password");
        else if (attributes.Password.Length < 8) errors.Add("Password too short");

        if (errors.Count > 0) throw new ValidationException(string.Join(", ", errors));

        return Task.CompletedTask;
    }
}