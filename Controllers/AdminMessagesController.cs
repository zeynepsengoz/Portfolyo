using Microsoft.AspNetCore.Mvc;
using PortfolyoDbContext;
using Portfolyo.Data;

namespace Portfolyo.Controllers
{
    public class AdminMessagesController : Controller
    {
        private readonly portfolyodbContext _context;

        public AdminMessagesController(portfolyodbContext context)
        {
            _context = context;
        }

        // Mesaj listesi
        public IActionResult Index()
        {
            var messages = _context.MessageTables
                .OrderByDescending(x => x.MessageId)
                .ToList();

            return View(messages);
        }

        [HttpGet]
        public IActionResult Detail(int id)
        {
            var message = _context.MessageTables.FirstOrDefault(x => x.MessageId == id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }



        [HttpPost]
        public IActionResult Delete(int id)
        {
            var message = _context.MessageTables.Find(id);
            if (message != null)
            {
                _context.MessageTables.Remove(message);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
