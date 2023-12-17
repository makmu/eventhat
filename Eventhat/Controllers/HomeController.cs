using Eventhat.Controllers.Dto;
using Eventhat.Database;
using Microsoft.AspNetCore.Mvc;

namespace Eventhat.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    private readonly ViewDataContext _viewData;

    public HomeController(ViewDataContext viewData)
    {
        _viewData = viewData;
    }

    [HttpGet("/")]
    public Task<ActionResult<PageDto>> LoadHomePageAsync()
    {
        var page = _viewData.Pages.SingleOrDefault(p => p.Name == "home");
        if (page == null) return Task.FromResult<ActionResult<PageDto>>(NotFound("Unknown page 'home'"));

        return Task.FromResult<ActionResult<PageDto>>(Ok(new PageDto(page.Data)));
    }
}