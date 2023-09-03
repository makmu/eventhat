using Eventhat.Database;
using Eventhat.Helpers;
using Eventhat.InfraStructure;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IMessageStreamDatabase _db;

    public AdminController(IMessageStreamDatabase db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public Task<ActionResult<IEnumerable<Guid>>> GetAdminUserIdsAsync()
    {
        return Task.FromResult<ActionResult<IEnumerable<Guid>>>(
            Ok(
                _db.AdminUsers.Select(u => u.Id)));
    }

    [HttpGet("users/{userId}")]
    public Task<ActionResult<AdminUserDto>> GetAdminUserByIdAsync(Guid userId)
    {
        return Task.FromResult<ActionResult<AdminUserDto>>(
            Ok(
                _db.AdminUsers
                    .Where(u => u.Id == userId)
                    .Select(u => new AdminUserDto(u.Id, u.Email, u.LoginCount))
                    .FirstOrDefault()));
    }

    [HttpGet("messages")]
    public Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesAsync()
    {
        return Task.FromResult<ActionResult<IEnumerable<MessageDto>>>(
            Ok(
                _db.Messages
                    .OrderBy(m => m.GlobalPosition)
                    .Select(m => new MessageDto(m.Id, m.Metadata.Deserialize<Metadata>().TraceId, m.Metadata.Deserialize<Metadata>().UserId, m.StreamName, m.Type, m.Time)
                    )));
    }

    [HttpGet("stream-messages")]
    public Task<ActionResult<IEnumerable<MessageDto>>> GetStreamMessages(string streamName)
    {
        return Task.FromResult<ActionResult<IEnumerable<MessageDto>>>(
            Ok(
                _db.Messages
                    .Where(m => m.StreamName == streamName)
                    .OrderBy(m => m.GlobalPosition)
                    .Select(m => new MessageDto(m.Id, m.Metadata.Deserialize<Metadata>().TraceId, m.Metadata.Deserialize<Metadata>().UserId, m.StreamName, m.Type, m.Time)
                    )));
    }

    [HttpGet("correlated-message")]
    public Task<ActionResult<IEnumerable<MessageDto>>> GetCorrelatedMessages(Guid correlationId)
    {
        return Task.FromResult<ActionResult<IEnumerable<MessageDto>>>(
            Ok(
                _db.Messages
                    .Where(m => m.Metadata.Deserialize<Metadata>().TraceId == correlationId)
                    .OrderBy(m => m.GlobalPosition)
                    .Select(m => new MessageDto(m.Id, m.Metadata.Deserialize<Metadata>().TraceId, m.Metadata.Deserialize<Metadata>().UserId, m.StreamName, m.Type, m.Time)
                    )));
    }

    [HttpGet("streams")]
    public Task<ActionResult<IEnumerable<AdminStreamDto>>> GetAdminStreams()
    {
        return Task.FromResult<ActionResult<IEnumerable<AdminStreamDto>>>(
            Ok(
                _db.AdminStreams
                    .Select(s => new AdminStreamDto(s.StreamName, s.MessageCount, s.LastMessageId))));
    }

    public class AdminUserDto
    {
        public AdminUserDto(Guid id, string email, int loginCount)
        {
            Id = id;
            Email = email;
            LoginCount = loginCount;
        }

        public Guid Id { get; }
        public string Email { get; }
        public int LoginCount { get; }
    }

    public class MessageDto
    {
        public MessageDto(Guid id, Guid correlationId, Guid userId, string stream, string type, DateTimeOffset timestamp)
        {
            Id = id;
            CorrelationId = correlationId;
            UserId = userId;
            Stream = stream;
            Type = type;
            Timestamp = timestamp;
        }

        public Guid Id { get; }
        public Guid CorrelationId { get; }
        public Guid UserId { get; }
        public string Stream { get; }
        public string Type { get; }
        public DateTimeOffset Timestamp { get; }
    }

    public class AdminStreamDto
    {
        public AdminStreamDto(string streamName, int messageCount, Guid lastMessageId)
        {
            StreamName = streamName;
            MessageCount = messageCount;
            LastMessageId = lastMessageId;
        }

        public string StreamName { get; }
        public int MessageCount { get; }
        public Guid LastMessageId { get; }
    }
}