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

        // Only one payment is processed at a time, so two tenants cannot
        // rent the same property (fine for a single-server pilot).
        private static readonly SemaphoreSlim rentLock = new(1, 1);

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

            // Already rented by this tenant
            var existing = await context.RentalApplications
                .FirstOrDefaultAsync(r => r.TenantId == user.Id
                    && r.PropertyId == propertyId
                    && r.Status == ApplicationStatus.Paid);
            if (existing != null)
            {
                TempData["Error"] = "You have already rented this property.";
                return RedirectToAction("MyRentals");
            }

            // Only published listings can be rented
            if (property.Status != PropertyStatus.Published)
            {
                TempData["Error"] = "This property is no longer available.";
                return RedirectToAction("Index", "Listing");
            }

            ViewBag.Balance = user.WalletBalance;
            ViewBag.Deposit = property.Price;
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int propertyId)
        {
            await rentLock.WaitAsync();
            try
            {
                // Load fresh data inside the lock
                var property = await context.Properties
                    .Include(p => p.Landlord)
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
                if (property == null) return NotFound();

                var tenant = await userManager.GetUserAsync(User);
                if (tenant == null)
                    return RedirectToAction("Login", "Account");

                // Cannot rent own property
                if (property.LandlordId == tenant.Id)
                {
                    TempData["Error"] = "You cannot rent your own property.";
                    return RedirectToAction("Detail", "Listing",
                        new { id = propertyId });
                }

                // Must still be available
                if (property.Status != PropertyStatus.Published)
                {
                    TempData["Error"] = "This property is no longer available.";
                    return RedirectToAction("Index", "Listing");
                }

                var landlord = await userManager.FindByIdAsync(property.LandlordId);
                if (landlord == null) return NotFound();

                // Check balance
                if (tenant.WalletBalance < property.Price)
                {
                    TempData["Error"] =
                        $"Insufficient balance. You need ₱{property.Price:N0} " +
                        $"but only have ₱{tenant.WalletBalance:N0}. " +
                        $"Please top up your wallet.";
                    return RedirectToAction("Apply", new { propertyId });
                }

                // All-or-nothing: if anything fails, nothing is saved
                await using var tx = await context.Database.BeginTransactionAsync();

                tenant.WalletBalance -= property.Price;
                landlord.WalletBalance += property.Price;
                await userManager.UpdateAsync(tenant);
                await userManager.UpdateAsync(landlord);

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

                property.Status = PropertyStatus.Rented;
                await context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Message"] =
                    $"Payment successful! You have rented {property.Title}. " +
                    $"₱{property.Price:N0} has been sent to the landlord.";
                return RedirectToAction("Receipt",
                    new { applicationId = application.RentalApplicationId });
            }
            finally
            {
                rentLock.Release();
            }
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