using Microsoft.AspNetCore.Mvc;

namespace Portfolyo.ViewComponents
{
    public class AdminSideBarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    
    }
}
