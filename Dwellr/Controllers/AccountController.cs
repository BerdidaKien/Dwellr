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
                    ActiveRole = model.Role
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
                if (user != null)
                {
                    // Remove old claims first to avoid duplicates
                    var existingClaims = await userManager.GetClaimsAsync(user);
                    var verificationClaim = existingClaims
                        .FirstOrDefault(c => c.Type == "VerificationStatus");
                    var roleClaim = existingClaims
                        .FirstOrDefault(c => c.Type == "ActiveRole");

                    if (verificationClaim != null)
                        await userManager.RemoveClaimAsync(user, verificationClaim);
                    if (roleClaim != null)
                        await userManager.RemoveClaimAsync(user, roleClaim);

                    // Add fresh claims
                    await userManager.AddClaimsAsync(user, new List<System.Security.Claims.Claim> {
                new System.Security.Claims.Claim(
                    "VerificationStatus",
                    user.VerificationStatus.ToString()),
                new System.Security.Claims.Claim(
                    "ActiveRole",
                    user.ActiveRole.ToString())
            });
                }

                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
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

        public IActionResult UploadID() => View();

        [HttpPost]
        public async Task<IActionResult> UploadID(IFormFile idDocument)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (idDocument != null && idDocument.Length > 0)
            {
                var uploads = Path.Combine("wwwroot", "images", "ids");
                Directory.CreateDirectory(uploads);
                var fileName = $"{user.Id}_{idDocument.FileName}";
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await idDocument.CopyToAsync(stream);
                user.IDDocumentPath = $"/images/ids/{fileName}";
                user.VerificationStatus = VerificationStatus.IDSubmitted;
                await userManager.UpdateAsync(user);
                TempData["Message"] = "ID uploaded successfully. Awaiting admin review.";
            }
            return RedirectToAction("Profile");
        }
    }
}