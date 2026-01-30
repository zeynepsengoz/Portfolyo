using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;

namespace Portfolyo.ViewComponents
{
    public class ExperinceViewComponent : ViewComponent
    {
        private readonly portfolyodbContext _portfolyodbContext;

        public ExperinceViewComponent(portfolyodbContext portfolyodbContext)
        {
            _portfolyodbContext = portfolyodbContext;
        }

        public IViewComponentResult Invoke()
        {
            var values = _portfolyodbContext.ServicesTables.ToList();
            return View(values);
        }
    }
}
