using Microsoft.AspNetCore.Mvc;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class MessagesController : Controller
    {
        private readonly portfolyodbContext _context;

        public MessagesController(portfolyodbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SendMessage(MessageTable message)
        {
            message.MessageDate = DateTime.Now;
           

            _context.MessageTables.Add(message);
            _context.SaveChanges();

            return RedirectToAction("Index", "Default");
        }
    }
}

