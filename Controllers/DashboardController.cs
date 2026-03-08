using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolyoDbContext;
using Microsoft.AspNetCore.Authorization;

namespace Portfolyo.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : Controller
    {
        private readonly portfolyodbContext _portfolyodbContext;

        public DashboardController(portfolyodbContext portfolyodbContext)
        {
            _portfolyodbContext = portfolyodbContext;
        }

        public IActionResult Index()
        {
            //ViewBag.FirstProject = _portfolyodbContext.ProjectsTables.FirstOrDefault().ProjectName;
            //ViewBag.LastProject = _portfolyodbContext.ProjectsTables.OrderByDescending(x => x.ProjectId).FirstOrDefault().ProjectName;

            //ViewBag.TotalServicesCount = _portfolyodbContext.ServicesTables.Count();

            //ViewBag.FirstCategory = _portfolyodbContext.CategoryTables.FirstOrDefault().CategoryName;

            ViewBag.MessageCount = _portfolyodbContext.MessageTables.Count();
            ViewBag.ProjectCount = _portfolyodbContext.ProjectsTables.Count();
            ViewBag.SkillCount =   _portfolyodbContext.SkillTables.Count();


            // Son 5 mesaj
            var lastMessages = _portfolyodbContext.MessageTables
                .OrderByDescending(x => x.MessageDate)
                .Take(5)
                .ToList();

            // Bugün gelenler (Özet kartı)
            var today = DateTime.Today;
            ViewBag.TodayNewMessages = _portfolyodbContext.MessageTables.Count(x => x.MessageDate >= today);

            // Projede CreatedDate yoksa bu satırı şimdilik 0 bırakacağız (aşağıda anlattım)
            ViewBag.TodayNewProjects = 0;

            return View(lastMessages);





        }
    }
}

