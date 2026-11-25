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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
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
                {
                    var existingClaims = await userManager.GetClaimsAsync(user);
                    var verificationClaim = existingClaims
                        .FirstOrDefault(c => c.Type == "VerificationStatus");
                    var roleClaim = existingClaims
                        .FirstOrDefault(c => c.Type == "ActiveRole");

                    if (verificationClaim != null)
                        await userManager.RemoveClaimAsync(user, verificationClaim);
                    if (roleClaim != null)
                        await userManager.RemoveClaimAsync(user, roleClaim);

                    await userManager.AddClaimsAsync(user, new List<Claim> {
                        new Claim("VerificationStatus",
                            user.VerificationStatus.ToString()),
                        new Claim("ActiveRole", user.ActiveRole.ToString())
                    });
                }

                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                ModelState.AddModelError("", "Invalid email or password");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
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