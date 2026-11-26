using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dwellr.Models;
using Dwellr.Models.ViewModels;
using System.Security.Claims;

namespace Dwellr.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public AccountController(UserManager<AppUser> um,
            SignInManager<AppUser> sm)
        {
            userManager = um;
            signInManager = sm;
        }

        public IActionResult Register() => View();

        // acceptPrivacy comes from the checkbox on the Register page
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model,
    bool acceptPrivacy)
        {
            if (!acceptPrivacy)
                ModelState.AddModelError("",
                    "Please accept the Privacy Notice to create an account.");

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    FullName = model.FullName,
                    UserName = model.Email,
                    Email = model.Email,
                    ActiveRole = model.Role,
                    // Experiment simplification: registering counts as verified.
                    // Landlords still need team approval to post listings.
                    VerificationStatus = VerificationStatus.EmailVerified
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await RefreshClaimsAsync(user);   // new line
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                // Banned accounts cannot log in
                if (user != null &&
                    user.VerificationStatus == VerificationStatus.Banned)
                {
                    ModelState.AddModelError("",
                        "This account has been banned.");
                    return View(model);
                }

                if (user != null)
                    await RefreshClaimsAsync(user);

                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                ModelState.AddModelError("", "Invalid email or password");
            }
            return View(model);
        }

        // POST only, so another site cannot log people out with a link or image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task RefreshClaimsAsync(AppUser user)
        {
            var types = new[] { "FullName", "VerificationStatus", "ActiveRole" };
            var oldClaims = (await userManager.GetClaimsAsync(user))
                .Where(c => types.Contains(c.Type))
                .ToList();
            await userManager.RemoveClaimsAsync(user, oldClaims);

            await userManager.AddClaimsAsync(user, new List<Claim> {
                new Claim("FullName", user.FullName ?? "Account"),
                new Claim("VerificationStatus", user.VerificationStatus.ToString()),
                new Claim("ActiveRole", user.ActiveRole.ToString())
            });
        }


        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        // ID uploads are disabled during the experiment.
        // The team verifies landlords manually.
        public IActionResult UploadID()
        {
            TempData["Message"] =
                "Landlord verification is handled by the Dwellr team. " +
                "We will contact you to verify your details.";
            return RedirectToAction("Profile");
        }
    }
}