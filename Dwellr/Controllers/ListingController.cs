using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    public class ListingController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public ListingController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public async Task<IActionResult> Index()
        {
            var properties = await context.Properties
                .Include(p => p.Photos)
                .Include(p => p.Landlord)
                .Where(p => p.Status == PropertyStatus.Published)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var property = await context.Properties
                .Include(p => p.Photos)
                .Include(p => p.Landlord)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null) return NotFound();

            // Allow landlord to view their own listings regardless of status
            var currentUserId = userManager.GetUserId(User);
            if (property.Status != PropertyStatus.Published
                && property.LandlordId != currentUserId)
                return NotFound();

            return View(property);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MarkAvailable(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var property = await context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null || property.LandlordId != user!.Id)
                return Forbid();

            property.Status = PropertyStatus.Published;
            await context.SaveChangesAsync();
            TempData["Message"] = "Property is now available again!";
            return RedirectToAction("MyListings");
        }

        [Authorize]
        public IActionResult Create() => View();

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Property model,
            List<IFormFile> photos)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.VerificationStatus != VerificationStatus.IDApproved)
            {
                ModelState.AddModelError("",
                    "You must be verified before posting a listing.");
                return View(model);
            }

            if (photos == null || photos.Count < 3)
            {
                ModelState.AddModelError("",
                    "You must upload at least 3 photos.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.LandlordId = user.Id;
                model.Status = PropertyStatus.Draft;
                context.Properties.Add(model);
                await context.SaveChangesAsync();

                foreach (var photo in photos)
                {
                    var uploads = Path.Combine("wwwroot", "images", "listings");
                    Directory.CreateDirectory(uploads);
                    var fileName = $"{model.PropertyId}_{photo.FileName}";
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await photo.CopyToAsync(stream);
                    context.PropertyPhotos.Add(new PropertyPhoto
                    {
                        PropertyId = model.PropertyId,
                        ImagePath = $"/images/listings/{fileName}",
                        IsCover = context.PropertyPhotos
                            .Count(pp => pp.PropertyId == model.PropertyId) == 0
                    });
                }
                await context.SaveChangesAsync();
                return RedirectToAction("MyListings");
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> MyListings()
        {
            var user = await userManager.GetUserAsync(User);
            var listings = await context.Properties
                .Include(p => p.Photos)
                .Where(p => p.LandlordId == user!.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(listings);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var property = await context.Properties
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null || property.LandlordId != user!.Id)
                return Forbid();

            if (property.Photos.Count < 3)
            {
                TempData["Error"] = "You need at least 3 photos to publish.";
                return RedirectToAction("MyListings");
            }

            property.Status = PropertyStatus.Published;
            await context.SaveChangesAsync();
            TempData["Message"] = "Listing published successfully!";
            return RedirectToAction("MyListings");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MarkRented(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var property = await context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null || property.LandlordId != user!.Id)
                return Forbid();

            property.Status = PropertyStatus.Rented;
            await context.SaveChangesAsync();
            TempData["Message"] = "Property marked as rented!";
            return RedirectToAction("MyListings");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Save(int propertyId)
        {
            var user = await userManager.GetUserAsync(User);
            var already = await context.SavedListings
                .AnyAsync(s => s.UserId == user!.Id
                    && s.PropertyId == propertyId);

            if (!already)
            {
                context.SavedListings.Add(new SavedListing
                {
                    UserId = user!.Id,
                    PropertyId = propertyId
                });
                await context.SaveChangesAsync();
                TempData["Message"] = "Listing saved!";
            }
            else
            {
                TempData["Error"] = "Already saved.";
            }
            return RedirectToAction("Detail", new { id = propertyId });
        }

        [Authorize]
        public async Task<IActionResult> Saved()
        {
            var user = await userManager.GetUserAsync(User);
            var saved = await context.SavedListings
                .Include(s => s.Property)
                .ThenInclude(p => p!.Photos)
                .Where(s => s.UserId == user!.Id)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();
            return View(saved);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Unsave(int propertyId)
        {
            var user = await userManager.GetUserAsync(User);
            var saved = await context.SavedListings
                .FirstOrDefaultAsync(s => s.UserId == user!.Id
                    && s.PropertyId == propertyId);
            if (saved != null)
            {
                context.SavedListings.Remove(saved);
                await context.SaveChangesAsync();
                TempData["Message"] = "Listing removed from saved.";
            }
            return RedirectToAction("Saved");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Report(int propertyId,
            string reportedUserId, string reason)
        {
            var user = await userManager.GetUserAsync(User);
            context.Reports.Add(new Report
            {
                ReporterId = user!.Id,
                ReportedUserId = reportedUserId,
                PropertyId = propertyId,
                Reason = reason
            });
            await context.SaveChangesAsync();
            TempData["Message"] = "Report submitted. Thank you!";
            return RedirectToAction("Detail", new { id = propertyId });
        }
    }
}