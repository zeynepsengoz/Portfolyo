using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;
using System.Linq;

public class HomeViewComponent : ViewComponent
{
    private readonly portfolyodbContext _context;

    public HomeViewComponent(portfolyodbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var about = _context.AboutMeTables.FirstOrDefault();
        return View(about);
    }
}
