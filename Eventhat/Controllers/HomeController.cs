using Eventhat.Controllers.Dto;
using Eventhat.Database;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    private readonly IMessageStreamDatabase _db;

    public HomeController(IMessageStreamDatabase db)
    {
        _db = db;
    }

    [HttpGet("/")]
    public Task<ActionResult<PageDto>> LoadHomePageAsync()
    {
        var page = _db.Pages.SingleOrDefault(p => p.Name == "home");
        if (page == null) return Task.FromResult<ActionResult<PageDto>>(NotFound("Unknown page 'home'"));

        return Task.FromResult<ActionResult<PageDto>>(Ok(new PageDto(page.Data)));
    }
}