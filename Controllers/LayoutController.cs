using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.Controllers
{
    public class LayoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

//Layout //hiçbir zaman tek başına çalışamaz neden -> renderBody() -> bizim sonradan oluşturacağımız içerik sayfalarını buranın içine atıyor