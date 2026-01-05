using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Services;
using EcoleApp.Utilitaires;
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
        private readonly ApplicationDbContext _db;

        public AccountController(
            AuthService authService,
            AuditService auditService,
            ApplicationDbContext db)
        {
            _authService = authService;
            _auditService = auditService;
            _db = db;
        }

        // =========================
        // LOGIN
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var utilisateur = await _authService
                    .VerifierIdentiteAsync(model.Email, model.MotDePasse);

                if (utilisateur == null)
                {
                    ModelState.AddModelError(string.Empty, "Identifiants invalides.");
                    return View(model);
                }

                // =========================
                // Session
                // =========================
                var sessionId = Guid.NewGuid().ToString();

                var session = new Session
                {
                    SessionId = sessionId,
                    UtilisateurId = utilisateur.Id,
                    StartTime = DateTime.UtcNow,
                    RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                };

                _db.Sessions.Add(session);
                await _db.SaveChangesAsync();

                // =========================
                // Claims
                // =========================
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.Id),
                    new Claim(ClaimTypes.Name, utilisateur.NomComplet),
                    new Claim(ClaimTypes.Email, utilisateur.Email),
                    new Claim(ClaimTypes.Role, utilisateur.Role!.NomRole),
                    new Claim("SessionId", sessionId)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                // =========================
                // Audit
                // =========================
                await _auditService.LogAsync(
                    utilisateur.Id,
                    "Connexion",
                    $"Connexion de l'utilisateur {utilisateur.Email}");

                // ✅ Redirection par rôle
                return RedirectParRole(utilisateur.Role.NomRole);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de la connexion.");
                Console.WriteLine(ex);
                return View(model);
            }
        }

        // =========================
        // LOGOUT
        // =========================
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = User.FindFirst("SessionId")?.Value;

            if (!string.IsNullOrEmpty(sessionId))
            {
                var session = await _db.Sessions.FindAsync(sessionId);
                if (session != null && session.EndTime == null)
                {
                    session.EndTime = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            await HttpContext.SignOutAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                await _auditService.LogAsync(
                    userId,
                    "Déconnexion",
                    "Déconnexion utilisateur");
            }

            return RedirectToAction(nameof(Login));
        }

        // =========================
        // ACCESS DENIED
        // =========================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =========================
        // CHANGE PASSWORD
        // =========================
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _db.Utilisateurs.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.MotDePasseHash))
            {
                ModelState.AddModelError(string.Empty, "Mot de passe actuel incorrect.");
                return View(model);
            }

            user.MotDePasseHash = PasswordHelper.HashPassword(model.NewPassword);
            user.MustChangePassword = false;

            await _db.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Changement mot de passe", "Mot de passe modifié");

            TempData["Success"] = "Mot de passe modifié avec succès.";

            // 🔹 Redirection selon rôle après changement de mot de passe
            return RedirectParRole(User.FindFirstValue(ClaimTypes.Role) ?? string.Empty);
        }

        // =========================
        // REDIRECTION PAR ROLE
        // =========================
        private IActionResult RedirectParRole(string role)
        {
            return role switch
            {
                Roles.Admin => RedirectToAction("Dashboard", "Admin"),
                Roles.Enseignant => RedirectToAction("Dashboard", "Responsable"),
                Roles.Delegue => RedirectToAction("Dashboard", "Delegue"),
                _ => RedirectToAction("Index", "Home")
            };
        }

    }
}
