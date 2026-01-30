using Microsoft.AspNetCore.Mvc;
using Portfolyo.Data;
using PortfolyoDbContext;
using System.Linq;

namespace Portfolyo.Controllers
{
    public class EducationController : Controller
    {
        private readonly portfolyodbContext _context;

        public EducationController(portfolyodbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var values = _context.EducationTables.ToList();
            return View(values);
        }

        // CREATE - FORM
        [HttpGet]
        public IActionResult Create()
        {
            return View(new EducationTable());
        }

        [HttpPost]
        public IActionResult Create(EducationTable education)
        {
            _context.EducationTables.Add(education);
            _context.SaveChanges();
            return RedirectToAction("Index", "Education");
        }

        [HttpGet]
        public IActionResult UpdateEducation(int id)
        {
            var education = _context.EducationTables.Find(id);
            return View(education);
        }

        [HttpPost]
        public IActionResult UpdateEducation(EducationTable education)
        {
            if (education.IsCurrent)
            {
                education.EndYear = null;
            }

            _context.EducationTables.Update(education);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult DeleteEducation(int id)
        {
            var education = _context.EducationTables.Find(id);
            _context.EducationTables.Remove(education);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
