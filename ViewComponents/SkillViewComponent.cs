using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;
using System.Linq;

public class SkillViewComponent : ViewComponent
{
    private readonly portfolyodbContext _context;

    public SkillViewComponent(portfolyodbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var skills = _context.SkillTables
            .OrderByDescending(x => x.Levels)
            .ToList();

        return View(skills);
    }
}
