using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;

public class PortfolioController : Controller
{

    private readonly portfolyodbContext _context;

    public PortfolioController(portfolyodbContext context)
    {
        _context = context;
    }

    public IActionResult Detail(int id)
    {
        var project = _context.ProjectsTables
            .Include(x => x.Category)
            .FirstOrDefault(x => x.ProjectId == id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }
}
