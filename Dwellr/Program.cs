using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Dwellr.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DwellrDbContext>(opts => {
    opts.UseSqlServer(
        builder.Configuration["ConnectionStrings:DwellrConnection"]);
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opts => {
    opts.Password.RequiredLength = 8;
    opts.Password.RequireNonAlphanumeric = false;
    opts.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<DwellrDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default",
    "{controller=Home}/{action=Index}/{id?}");

// Seed admin account
using (var scope = app.Services.CreateScope())
{
    await SeedData.EnsureAdminAsync(scope.ServiceProvider);
}

app.Run();