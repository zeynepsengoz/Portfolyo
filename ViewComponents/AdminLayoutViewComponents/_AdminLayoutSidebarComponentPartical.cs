using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.ViewComponents.AdminLayoutViewComponents
{
    public class _AdminLayoutSidebarComponentPartical : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
