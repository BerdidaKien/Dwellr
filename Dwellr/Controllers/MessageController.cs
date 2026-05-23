using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public MessageController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var messages = await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Property)
                .Where(m => m.SenderId == user!.Id
                    || m.ReceiverId == user!.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return View(messages);
        }

        public async Task<IActionResult> Conversation(int propertyId,
            string otherUserId)
        {
            var user = await userManager.GetUserAsync(User);
            var messages = await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.PropertyId == propertyId &&
                    ((m.SenderId == user!.Id && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == user!.Id)))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.PropertyId = propertyId;
            ViewBag.OtherUserId = otherUserId;
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send(string receiverId,
            int propertyId, string body)
        {
            var user = await userManager.GetUserAsync(User);

            if (user!.Id == receiverId)
            {
                TempData["Error"] = "You cannot message yourself.";
                return RedirectToAction("Index");
            }

            if (user.VerificationStatus == VerificationStatus.Unverified)
            {
                TempData["Error"] =
                    "You must verify your email before messaging.";
                return RedirectToAction("Profile", "Account");
            }

            var message = new Message
            {
                SenderId = user.Id,
                ReceiverId = receiverId,
                PropertyId = propertyId,
                Body = body,
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();
            return RedirectToAction("Conversation",
                new { propertyId, otherUserId = receiverId });
        }
    }
}