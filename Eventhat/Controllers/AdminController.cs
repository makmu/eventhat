using Eventhat.Database;
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
}