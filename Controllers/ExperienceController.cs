using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class ExperienceController : Controller
    {

        private readonly portfolyodbContext _portfolyodbContext;

        public ExperienceController(portfolyodbContext portfolyodbContext)
        {
            _portfolyodbContext = portfolyodbContext;
        }

        //Verileri burada listeleyeceğiz
        public IActionResult Index()
        {
           
            return View();
        }

        //Create metotu için get fonksiyonu
        public IActionResult CreateExperience()
        {
            return View();
        }

        //Güncelleme metotu için get fonksiyonu
        //HttpGet HttpPost

        //Burası Get Fonksiyonu Update için
        public IActionResult UpdateExperience()
        {
            return View();
        }
        
        [HttpPost]
        //public IActionResult UpdateExperince()
        //{
        //    //Buraya veri tabanına kaydetmek için gerekli kodlar gelecek
        //}
        //[HttpPost]
        public IActionResult DeleteExperience()
        {
            return View(); //değişecek
        }
    }
}
