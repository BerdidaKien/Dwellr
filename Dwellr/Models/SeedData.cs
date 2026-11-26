using Microsoft.AspNetCore.Identity;

namespace Dwellr.Models
{
    public static class SeedData
    {
        public static async Task EnsureAdminAsync(IServiceProvider services)
        {
            var config = services.GetRequiredService<IConfiguration>();
            var env = services.GetRequiredService<IHostEnvironment>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            var adminEmail = "admin@dwellr.com";
            var adminPassword = config["Seed:AdminPassword"] ?? "Admin@1234";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    FullName = "Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    VerificationStatus = VerificationStatus.IDApproved,
                    ActiveRole = UserRole.Both
                };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            var seedDemo = config.GetValue<bool?>("Seed:DemoAccounts")
                ?? env.IsDevelopment();
            if (!seedDemo) return;

            if (await userManager.FindByEmailAsync("landlord@dwellr.com") == null)
            {
                var landlord = new AppUser
                {
                    FullName = "Test Landlord",
                    UserName = "landlord@dwellr.com",
                    Email = "landlord@dwellr.com",
                    VerificationStatus = VerificationStatus.IDApproved,
                    ActiveRole = UserRole.Landlord
                };
                await userManager.CreateAsync(landlord, "Landlord@1234");
            }

            if (await userManager.FindByEmailAsync("tenant@dwellr.com") == null)
            {
                var tenant = new AppUser
                {
                    FullName = "Test Tenant",
                    UserName = "tenant@dwellr.com",
                    Email = "tenant@dwellr.com",
                    VerificationStatus = VerificationStatus.EmailVerified,
                    ActiveRole = UserRole.Tenant
                };
                await userManager.CreateAsync(tenant, "Tenant@1234");
            }
        }
    }
}