using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

namespace Dwellr.Controllers
{
    public class HomeController : Controller
    {
        private readonly DwellrDbContext context;
        private readonly UserManager<AppUser> userManager;

        public HomeController(DwellrDbContext ctx,
            UserManager<AppUser> um)
        {
            context = ctx;
            userManager = um;
        }

        public async Task<IActionResult> Index(string? city,
            string? district, PropertyType? type,
            decimal? minPrice, decimal? maxPrice)
        {

            var query = context.Properties
                .Include(p => p.Photos)
                .Include(p => p.Landlord)
                .Where(p => p.Status == PropertyStatus.Published);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(p => p.City == city);

            if (!string.IsNullOrEmpty(district))
                query = query.Where(p => p.District == district);

            if (type.HasValue)
                query = query.Where(p => p.Type == type);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Cities = await context.Properties
                .Where(p => p.Status == PropertyStatus.Published)
                .Select(p => p.City)
                .Distinct()
                .ToListAsync();

            ViewBag.SelectedCity = city;
            ViewBag.SelectedType = type;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(properties);
        }
    }
}