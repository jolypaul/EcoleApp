using EcoleApp.Services;
using EcoleApp.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoleApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly AuditService _auditService;

        public AccountController(AuthService authService, AuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var utilisateur = await _authService
                .VerifierIdentiteAsync(model.Email, model.MotDePasse);

            if (utilisateur == null)
            {
                ModelState.AddModelError("", "Identifiants invalides.");
                return View(model);
            }

            // Création des claims (jeton de session)
            var claims = new List<Claim>
            {
                new Claim("UserId", utilisateur.Id),
                new Claim(ClaimTypes.Name, utilisateur.NomComplet),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Role, utilisateur.Role!.NomRole)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // Audit connexion
            await _auditService.LogAsync(
                utilisateur.Id,
                "Connexion",
                $"Connexion de l'utilisateur {utilisateur.Email}"
            );

            // Redirection selon le rôle
            return utilisateur.Role.NomRole == "Admin"
                ? RedirectToAction("CreateUser", "Admin")
                : RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("UserId")?.Value;

            await HttpContext.SignOutAsync();

            if (userId != null)
            {
                await _auditService.LogAsync(
                    userId,
                    "Déconnexion",
                    "Déconnexion utilisateur"
                );
            }

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
