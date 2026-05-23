using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;
using Dwellr.Models.ViewModels;

namespace Dwellr.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public WalletController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var transactions = await context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Include(t => t.Property)
                .Where(t => t.SenderId == user.Id
                    || t.ReceiverId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Balance = user.WalletBalance;
            ViewBag.UserId = user.Id;
            return View(transactions);
        }

        public IActionResult TopUp() => View(new TopUpViewModel());

        [HttpPost]
        public async Task<IActionResult> TopUp(TopUpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Simulate card processing
            // In real app this would call a payment gateway
            user.WalletBalance += model.Amount;
            await userManager.UpdateAsync(user);

            // Record transaction
            context.Transactions.Add(new Transaction
            {
                Amount = model.Amount,
                Type = TransactionType.TopUp,
                Status = TransactionStatus.Completed,
                ReceiverId = user.Id,
                Description = $"Wallet top up via card " +
                    $"****{model.CardNumber.Substring(12)}"
            });
            await context.SaveChangesAsync();

            TempData["Message"] =
                $"₱{model.Amount:N0} added to your wallet successfully!";
            return RedirectToAction("Index");
        }
    }
}