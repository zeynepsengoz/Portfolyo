using Microsoft.AspNetCore.Mvc;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class ServicesController : Controller
    {
        private readonly portfolyodbContext _portfolyodbContext;

        public ServicesController(portfolyodbContext portfolyodbContext)
        {
            _portfolyodbContext = portfolyodbContext;
        }

        public IActionResult Index()
        {
            var values = _portfolyodbContext.ServicesTables.ToList();
            return View(values);
        }

        [HttpGet]

        public IActionResult CreateServices()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateServices(ServicesTable services)
        {
            _portfolyodbContext.ServicesTables.Add(services);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]

        public IActionResult UpdateServices(int id)
        {
            var value = _portfolyodbContext.ServicesTables.Find(id);
            return View(value);
        }

        [HttpPost]

        public IActionResult UpdateServices(ServicesTable services)
        {
            _portfolyodbContext.ServicesTables.Update(services);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");

        }

        public IActionResult DeleteServices(int id)
        {
            var value = _portfolyodbContext.ServicesTables.Find(id);
            _portfolyodbContext.ServicesTables.Remove(value);

            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}

