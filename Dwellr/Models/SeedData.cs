using Microsoft.AspNetCore.Identity;

namespace Dwellr.Models
{
    public static class SeedData
    {
        public static async Task EnsureAdminAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Create Admin role
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Admin account
            var adminEmail = "admin@dwellr.com";
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
                await userManager.CreateAsync(admin, "Admin@1234");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Create Test Landlord
            var landlordEmail = "landlord@dwellr.com";
            var landlord = await userManager.FindByEmailAsync(landlordEmail);
            if (landlord == null)
            {
                landlord = new AppUser
                {
                    FullName = "Test Landlord",
                    UserName = landlordEmail,
                    Email = landlordEmail,
                    VerificationStatus = VerificationStatus.IDApproved,
                    ActiveRole = UserRole.Landlord
                };
                await userManager.CreateAsync(landlord, "Landlord@1234");
            }

            // Create Test Tenant
            var tenantEmail = "tenant@dwellr.com";
            var tenant = await userManager.FindByEmailAsync(tenantEmail);
            if (tenant == null)
            {
                tenant = new AppUser
                {
                    FullName = "Test Tenant",
                    UserName = tenantEmail,
                    Email = tenantEmail,
                    VerificationStatus = VerificationStatus.EmailVerified,
                    ActiveRole = UserRole.Tenant
                };
                await userManager.CreateAsync(tenant, "Tenant@1234");
            }
        }
    }
}