using Microsoft.AspNetCore.Mvc;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{

    public class SkillNewController : Controller
    {

      
        private readonly portfolyodbContext _portfolyodbContext;

        public SkillNewController(portfolyodbContext portfolyodbContext)
        {
            _portfolyodbContext = portfolyodbContext;
        }
        //public IActionResult SkillList()
        //{
        //    var values = _portfolyodbContext.SkillTables.ToList();
        //    return View(values);
        //}

        public IActionResult Index()
        {
            return RedirectToAction("SkillList");
        }

        public IActionResult SkillList()
        {
            var values = _portfolyodbContext.SkillTables.ToList();
            return View(values);
        }


        [HttpGet]
        public IActionResult CreateSkill()
        {
            return View();  
        }

        [HttpPost]
        public IActionResult CreateSkill(SkillTable skill)
        {
            _portfolyodbContext.SkillTables.Add(skill);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("SkillList", "SkillNew");
        }

        [HttpGet]
        public IActionResult UpdateSkill(int id)
        {
            var skill = _portfolyodbContext.SkillTables.Find(id);
            return View(skill);
        }

        [HttpPost]
        public IActionResult UpdateSkill(SkillTable skill)
        {
            _portfolyodbContext.SkillTables.Update(skill);
            _portfolyodbContext.SaveChanges();
            return RedirectToAction("SkillList","SkillNew");
        }

        public IActionResult DeleteSkill(int id)
        {
            var skill = _portfolyodbContext.SkillTables.Find(id);
            _portfolyodbContext.SkillTables.Remove(skill);
            _portfolyodbContext.SaveChanges();

            return RedirectToAction("SkillList","SkillNew");
        }

        

    }
}

