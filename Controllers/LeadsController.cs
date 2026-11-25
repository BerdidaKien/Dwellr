using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    public class LeadsController : Controller
    {
        private readonly DwellrDbContext context;

        public LeadsController(DwellrDbContext ctx)
        {
            context = ctx;
        }

        // ---------- Experiment 1: tenant waitlist ----------
        public IActionResult Waitlist(string? src) =>
            View(new WaitlistEntry { Source = src });

        [HttpPost]
        public async Task<IActionResult> Waitlist(WaitlistEntry model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            if (!await context.WaitlistEntries.AnyAsync(w => w.Email == email))
            {
                model.Email = email;
                model.CreatedAt = DateTime.UtcNow;
                context.WaitlistEntries.Add(model);
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Thanks");
        }

        // ---------- Experiment 2: landlord concierge ----------
        public IActionResult ListYourProperty() => View(new ListingRequest());

        [HttpPost]
        public async Task<IActionResult> ListYourProperty(ListingRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Status = LeadStatus.New;
            model.AdminNotes = null;
            model.CreatedAt = DateTime.UtcNow;
            context.ListingRequests.Add(model);
            await context.SaveChangesAsync();
            return RedirectToAction("Thanks", new { landlord = true });
        }

        public IActionResult Thanks(bool landlord = false)
        {
            ViewBag.Landlord = landlord;
            return View();
        }

        // ---------- Admin: see results ----------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Waitlist = await context.WaitlistEntries
                .OrderByDescending(w => w.CreatedAt).ToListAsync();
            var requests = await context.ListingRequests
                .OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(requests);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id,
            LeadStatus status, string? adminNotes)
        {
            var request = await context.ListingRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = status;
                request.AdminNotes = adminNotes;
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard");
        }
    }
}