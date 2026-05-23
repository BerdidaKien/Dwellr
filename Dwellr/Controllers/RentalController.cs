using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public RentalController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public async Task<IActionResult> Apply(int propertyId)
        {
            var property = await context.Properties
                .Include(p => p.Photos)
                .Include(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (property == null) return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Cannot rent own property
            if (property.LandlordId == user.Id)
            {
                TempData["Error"] = "You cannot rent your own property.";
                return RedirectToAction("Detail", "Listing",
                    new { id = propertyId });
            }

            // Check if already applied
            var existing = await context.RentalApplications
                .FirstOrDefaultAsync(r => r.TenantId == user.Id
                    && r.PropertyId == propertyId
                    && r.Status == ApplicationStatus.Paid);
            if (existing != null)
            {
                TempData["Error"] = "You have already rented this property.";
                return RedirectToAction("Detail", "Listing",
                    new { id = propertyId });
            }

            ViewBag.Balance = user.WalletBalance;
            ViewBag.Deposit = property.Price;
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int propertyId)
        {
            var property = await context.Properties
                .Include(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (property == null) return NotFound();

            var tenant = await userManager.GetUserAsync(User);
            if (tenant == null)
                return RedirectToAction("Login", "Account");

            var landlord = await userManager.FindByIdAsync(property.LandlordId);
            if (landlord == null) return NotFound();

            // Cannot rent own property
            if (property.LandlordId == tenant.Id)
            {
                TempData["Error"] = "You cannot rent your own property.";
                return RedirectToAction("Detail", "Listing",
                    new { id = propertyId });
            }

            // Check balance
            if (tenant.WalletBalance < property.Price)
            {
                TempData["Error"] =
                    $"Insufficient balance. You need ₱{property.Price:N0} " +
                    $"but only have ₱{tenant.WalletBalance:N0}. " +
                    $"Please top up your wallet.";
                return RedirectToAction("Apply", new { propertyId });
            }

            // Process payment
            tenant.WalletBalance -= property.Price;
            landlord.WalletBalance += property.Price;

            await userManager.UpdateAsync(tenant);
            await userManager.UpdateAsync(landlord);

            // Create transaction record
            var transaction = new Transaction
            {
                Amount = property.Price,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                SenderId = tenant.Id,
                ReceiverId = landlord.Id,
                PropertyId = propertyId,
                Description = $"Security deposit for {property.Title}"
            };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            // Create rental application
            var application = new RentalApplication
            {
                TenantId = tenant.Id,
                PropertyId = propertyId,
                Status = ApplicationStatus.Paid,
                DepositAmount = property.Price,
                PaidAt = DateTime.UtcNow,
                TransactionId = transaction.TransactionId
            };
            context.RentalApplications.Add(application);

            // Mark property as rented
            property.Status = PropertyStatus.Rented;
            await context.SaveChangesAsync();

            TempData["Message"] =
                $"Payment successful! You have rented {property.Title}. " +
                $"₱{property.Price:N0} has been sent to the landlord.";
            return RedirectToAction("Receipt",
                new { applicationId = application.RentalApplicationId });
        }

        public async Task<IActionResult> Receipt(int applicationId)
        {
            var application = await context.RentalApplications
                .Include(r => r.Property)
                .ThenInclude(p => p!.Landlord)
                .Include(r => r.Tenant)
                .Include(r => r.Transaction)
                .FirstOrDefaultAsync(r =>
                    r.RentalApplicationId == applicationId);

            if (application == null) return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (application.TenantId != user!.Id) return Forbid();

            return View(application);
        }

        public async Task<IActionResult> MyRentals()
        {
            var user = await userManager.GetUserAsync(User);
            var rentals = await context.RentalApplications
                .Include(r => r.Property)
                .ThenInclude(p => p!.Photos)
                .Include(r => r.Property)
                .ThenInclude(p => p!.Landlord)
                .Include(r => r.Transaction)
                .Where(r => r.TenantId == user!.Id)
                .OrderByDescending(r => r.AppliedAt)
                .ToListAsync();
            return View(rentals);
        }
    }
}