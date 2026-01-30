using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;
using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Portfolyo.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly portfolyodbContext _portfolyodbContext;

        public ProjectsController(portfolyodbContext portfolyodbcontext)
        {
            _portfolyodbContext = portfolyodbcontext;
        }
        public IActionResult Index()
        {
            //Include = Projeleri çekiyorum ama kategorisi de gelsin istiyorum.
            //Lambda --> İlgili projenin kategorisini diğer tablodan çekmek için kullanıyoruz

            //Garson Örneği 
            //Bana projeleri getir
            //Yanında ne olsun
            //Kategorisi de olsun

            var value = _portfolyodbContext.ProjectsTables.Include(x => x.Category).ToList();

            return View(value);
        }




        [HttpGet]

        public IActionResult ProjectCreate()
        {

            //viewbag category---> her kategori için bir listeye veri ekleyecek bu listenin içerisinde bir gözüken kısım değeri ikinci olarak value değeri olucak
            //bunları dropdownda kullanacağız

            // liste[0] -> Text = web -- value = 1
            // liste[1] -> Text = mobil -- value = 2


            ViewBag.Category = _portfolyodbContext.CategoryTables.Select(x => new SelectListItem
            {
                Text = x.CategoryName,
                Value = x.CategoryId.ToString()
            })
                .ToList()
            ;

            return View();
        }

        [HttpPost]

        public IActionResult ProjectCreate(ProjectsTable projectsTable)
        {
            _portfolyodbContext.ProjectsTables.Add(projectsTable);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ProjectUpdate(int id)
        {
            // Güncellenecek projeyi getir
            var project = _portfolyodbContext.ProjectsTables.Find(id);

            // Kategori dropdown doldur
            ViewBag.Category = _portfolyodbContext.CategoryTables
                .Select(x => new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                })
                .ToList();

            return View(project);
        }

        [HttpPost]
        public IActionResult ProjectUpdate(ProjectsTable projectsTable)
        {
            _portfolyodbContext.ProjectsTables.Update(projectsTable);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]

        public IActionResult ProjectDelete(int id)
        {
            var project = _portfolyodbContext.ProjectsTables.Find(id);
            _portfolyodbContext.ProjectsTables.Remove(project);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult ProjectDelete(ProjectsTable projectsTable)
        {
            _portfolyodbContext.ProjectsTables.Remove(projectsTable);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }


    }

}
