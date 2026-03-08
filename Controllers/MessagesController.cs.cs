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
        public async Task<IActionResult> SendMessage(MessageTable message)
        {
            message.MessageDate = DateTime.Now;

            _context.MessageTables.Add(message);
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] = "Your message has been sent. Thank you, I will get back to you shortly.";
            return Redirect("/Default/Index?sent=1#contact");
        }
    }
}
