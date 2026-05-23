using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public AdminController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            var users = userManager.Users.ToList();
            return View(users);
        }

        public async Task<IActionResult> PendingVerifications()
        {
            var pending = userManager.Users
                .Where(u => u.VerificationStatus == VerificationStatus.IDSubmitted)
                .ToList();
            return View(pending);
        }

        public async Task<IActionResult> Reports()
        {
            var reports = await context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Property)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveID(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.VerificationStatus = VerificationStatus.IDApproved;
                user.ActiveRole = UserRole.Landlord;
                await userManager.UpdateAsync(user);

                // Refresh claims
                var existingClaims = await userManager.GetClaimsAsync(user);
                var verificationClaim = existingClaims
                    .FirstOrDefault(c => c.Type == "VerificationStatus");
                var roleClaim = existingClaims
                    .FirstOrDefault(c => c.Type == "ActiveRole");

                if (verificationClaim != null)
                    await userManager.RemoveClaimAsync(user, verificationClaim);
                if (roleClaim != null)
                    await userManager.RemoveClaimAsync(user, roleClaim);

                await userManager.AddClaimsAsync(user, new List<System.Security.Claims.Claim> {
            new System.Security.Claims.Claim(
                "VerificationStatus",
                VerificationStatus.IDApproved.ToString()),
            new System.Security.Claims.Claim(
                "ActiveRole",
                UserRole.Landlord.ToString())
        });

                TempData["Message"] = $"{user.FullName} has been approved as a Landlord.";
            }
            return RedirectToAction("PendingVerifications");
        }

        [HttpPost]
        public async Task<IActionResult> RejectID(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.VerificationStatus = VerificationStatus.Unverified;
                await userManager.UpdateAsync(user);
                TempData["Message"] = $"{user.FullName} has been rejected.";
            }
            return RedirectToAction("PendingVerifications");
        }

        [HttpPost]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.VerificationStatus = VerificationStatus.Banned;
                await userManager.UpdateAsync(user);

                var listings = await context.Properties
                    .Where(p => p.LandlordId == userId)
                    .ToListAsync();
                foreach (var listing in listings)
                    listing.Status = PropertyStatus.Hidden;

                await context.SaveChangesAsync();
                TempData["Message"] = $"{user.FullName} has been banned.";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ActionReport(int reportId)
        {
            var report = await context.Reports.FindAsync(reportId);
            if (report != null)
            {
                report.Status = ReportStatus.Actioned;
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Reports");
        }
    }
}