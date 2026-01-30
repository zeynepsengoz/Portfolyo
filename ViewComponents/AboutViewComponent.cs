using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;

public class AboutViewComponent : ViewComponent
{
    private readonly portfolyodbContext _context;

    public AboutViewComponent(portfolyodbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var model = new AboutViewModel
        {
            Info = _context.AboutInfoTables.FirstOrDefault(),
            Educations = _context.EducationTables
                .OrderByDescending(x => x.StartYear)
                .ToList()
        };

        return View(model);
    }
}
