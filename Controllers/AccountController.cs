using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Services;
using EcoleApp.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EcoleApp.Utilitaires;

namespace EcoleApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly AuditService _auditService;
        private readonly ApplicationDbContext _db;

        public AccountController(AuthService authService, AuditService auditService, ApplicationDbContext db)
        {
            _authService = authService;
            _auditService = auditService;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

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
                    ModelState.AddModelError("", "Identifiants invalides.");
                    return View(model);
                }

                // create session id and record session before sign-in
                var sessionId = Guid.NewGuid().ToString();
                var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var session = new Session
                {
                    SessionId = sessionId,
                    UtilisateurId = utilisateur.Id,
                    StartTime = DateTime.UtcNow,
                    RemoteIp = remoteIp,
                    UserAgent = userAgent
                };

                _db.Sessions.Add(session);
                await _db.SaveChangesAsync();

                // Création des claims (jeton de session)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.Id),
                    new Claim("UserId", utilisateur.Id),
                    new Claim("SessionId", sessionId),
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
            catch (InvalidOperationException dbEx)
            {
                // Database not ready / migration problems
                ModelState.AddModelError(string.Empty, "Erreur d'accès à la base de données. Vérifiez que la base et les migrations existent. Détails: " + dbEx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                // Generic fallback
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la tentative de connexion.");
                Console.WriteLine(ex);
                return View(model);
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId")?.Value;
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
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _db.Utilisateurs.FindAsync(userId);
            if (user == null) return NotFound();

            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.MotDePasseHash))
            {
                ModelState.AddModelError(string.Empty, "Mot de passe actuel invalide.");
                return View(model);
            }

            user.MotDePasseHash = PasswordHelper.HashPassword(model.NewPassword);
            user.MustChangePassword = false;
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Changement mot de passe", "Utilisateur a changé son mot de passe.");

            TempData["Success"] = "Mot de passe modifié.";
            return RedirectToAction("Index", "Home");
        }
    }
}
