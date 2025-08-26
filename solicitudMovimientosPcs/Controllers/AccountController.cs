using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using solicitudMovimientosPcs.Models.Account;
using solicitudMovimientosPcs.Services;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ActiveDirectoryService _adService;
    public AccountController(ActiveDirectoryService adService) => _adService = adService;

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var displayName = _adService.GetDisplayName(vm.Username, vm.Password);
        if (!string.IsNullOrEmpty(displayName))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, vm.Username),
                new Claim("DisplayName", displayName),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var props = new AuthenticationProperties { IsPersistent = vm.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return LocalRedirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.ErrorMessage = "Usuario o contraseña incorrectos.";
        return View(vm);
    }
}
