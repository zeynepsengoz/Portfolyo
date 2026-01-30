using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
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



            // Task
            // 1- En çok kategoriye sahip proje
            // 2- Yetenekler tablosunda en yüksek yüzdeye sahip yetenek

            return View();
        }
    }
}

