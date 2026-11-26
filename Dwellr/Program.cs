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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DwellrDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureAdminAsync(scope.ServiceProvider);
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();