using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.Controllers
{
    public class AdminLayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
