using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.ViewComponents
{
    public class AdminHeadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
