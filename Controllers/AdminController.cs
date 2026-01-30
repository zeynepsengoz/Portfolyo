
using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;

public class AdminController : Controller
{
    private readonly portfolyodbContext _context;

    public AdminController(portfolyodbContext context)
    {
        _context = context;
    }

    public IActionResult About()
    {
        var about = _context.AboutMeTables.FirstOrDefault();
        var info = _context.AboutInfoTables.FirstOrDefault();

        var model = new AboutViewModel
        {
            About = about,
            Info = info
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult About(AboutViewModel model)
    {
        // AboutMeTable (Ad, Ünvan, Foto)
        var about = _context.AboutMeTables.FirstOrDefault();
        if (about == null)
            _context.AboutMeTables.Add(model.About);
        else
        {
            about.NameSurname = model.About.NameSurname;
            about.JobTitle = model.About.JobTitle;
            about.ImagePath = model.About.ImagePath;
            about.ShortDescription = model.About.ShortDescription;

        }

        // AboutInfoTable (Hakkımda detay)
        var info = _context.AboutInfoTables.FirstOrDefault();
        if (info == null)
            _context.AboutInfoTables.Add(model.Info);
        else
        {
            info.LongDescription = model.Info.LongDescription;
            info.Age = model.Info.Age;
            info.Email = model.Info.Email;
            info.Interests = model.Info.Interests;
        }

        _context.SaveChanges();
        return RedirectToAction("About");
    }
}
