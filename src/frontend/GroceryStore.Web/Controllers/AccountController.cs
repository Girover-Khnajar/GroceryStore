using System.Security.Claims;
using GroceryStore.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    private readonly IConfiguration _config;

    public AccountController(IConfiguration config) => _config = config;

    // ── GET /Account/Login ─────────────────────────────────────────────
    [HttpGet("Login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Admin");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ── POST /Account/Login ────────────────────────────────────────────
    [HttpPost("Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var adminUser = _config["AdminCredentials:Username"] ?? "admin";
        var adminPass = _config["AdminCredentials:Password"] ?? "admin123";

        if (model.Username != adminUser || model.Password != adminPass)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,  model.Username),
            new Claim(ClaimTypes.Role, "Admin"),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        var returnUrl = model.ReturnUrl;
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Dashboard", "Admin");
    }

    // ── POST /Account/Logout ───────────────────────────────────────────
    [HttpPost("Logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
