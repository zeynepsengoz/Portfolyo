using Microsoft.AspNetCore.Mvc;
using Portfolyo.Data;
using Portfolyo.Services;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class MessagesController : Controller
    {
        private readonly portfolyodbContext _context;
        private readonly EmailService _emailService;

        public MessagesController(portfolyodbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageTable message)
        {
            message.MessageDate = DateTime.Now;

            _context.MessageTables.Add(message);
            await _context.SaveChangesAsync();

            await _emailService.SendContactAutoReplyAsync(message.Email, message.Name);

            return RedirectToAction("Index", "Default");
        }
    }
}
