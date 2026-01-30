using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;

public class AboutController : Controller
{
    private readonly portfolyodbContext _context;

    public AboutController(portfolyodbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var model = new AboutViewModel
        {
            About = _context.AboutMeTables.FirstOrDefault(),
            Info = _context.AboutInfoTables.FirstOrDefault()
        };

        return View(model);
    }
}
