//using Microsoft.AspNetCore.Mvc;
////Kütüphaneleri çağırıyor

//namespace Portfolyo.Controllers
//{
//    public class DefaultController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class DefaultController : Controller
    {
        private readonly portfolyodbContext _context;

        public DefaultController(portfolyodbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var projects = _context.ProjectsTables
                .Include(x => x.Category)
                .Include(x => x.ProjectImages)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ProjectId)
                .ToList();

            return View(projects);
        }
    }
}

