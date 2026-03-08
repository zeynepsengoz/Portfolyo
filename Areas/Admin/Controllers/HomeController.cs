using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
[Route("admin")]
public class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect("/Dashboard/Index");
    }
}
