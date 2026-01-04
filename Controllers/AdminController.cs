using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Services;
using EcoleApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EcoleApp.Controllers
{
    [Authorize(Roles = "Admin,Responsable")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminService _adminService;
        private readonly AuditService _auditService;
        private readonly ImportService _importService;

        public AdminController(
            ApplicationDbContext context,
            AdminService adminService,
            AuditService auditService,
            ImportService importService)
        {
            _context = context;
            _adminService = adminService;
            _auditService = auditService;
            _importService = importService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult CreateUser()
        {
            ViewBag.Roles = new SelectList(_context.Roles, "Id", "NomRole");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_context.Roles, "Id", "NomRole");
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId")?.Value ?? string.Empty;

            try
            {
                await _adminService.CreerUtilisateurAsync(
                    model.Nom,
                    model.Email,
                    model.MotDePasse,
                    model.RoleId,
                    adminId
                );

                TempData["Success"] = "Utilisateur créé avec succès";
                return RedirectToAction(nameof(CreateUser));
            }
            catch (Exception ex)
            {
                // Handle business errors (email exists, etc.)
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Roles = new SelectList(_context.Roles, "Id", "NomRole");
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ChangeRole()
        {
            ViewBag.Utilisateurs = new SelectList(
                _context.Utilisateurs, "Id", "Email");

            ViewBag.Roles = new SelectList(
                _context.Roles, "Id", "NomRole");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Utilisateurs = new SelectList(
                    _context.Utilisateurs, "Id", "Email");

                ViewBag.Roles = new SelectList(
                    _context.Roles, "Id", "NomRole");

                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId")?.Value ?? string.Empty;

            try
            {
                await _adminService.ChangerRoleAsync(
                    model.UtilisateurId,
                    model.NouveauRoleId,
                    adminId
                );

                TempData["Success"] = "Rôle modifié avec succès.";
                return RedirectToAction(nameof(ChangeRole));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Utilisateurs = new SelectList(
                    _context.Utilisateurs, "Id", "Email");

                ViewBag.Roles = new SelectList(
                    _context.Roles, "Id", "NomRole");

                return View(model);
            }
        }

        // ======================
        // Roles management (UC5)
        // ======================
        public IActionResult Roles()
        {
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        public IActionResult EditRole(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var role = _context.Roles.Find(id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(Role model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = _context.Roles.Find(model.Id);
            if (role == null)
                return NotFound();

            role.NomRole = model.NomRole;
            role.Responsabilite = model.Responsabilite;
            await _context.SaveChangesAsync();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _auditService.LogAsync(adminId, "Modification rôle", $"Rôle {role.NomRole} mis à jour.");

            TempData["Success"] = "Rôle mis à jour.";
            return RedirectToAction(nameof(Roles));
        }

        // ======================
        // Settings (UC3) - placeholder
        // ======================
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(IFormCollection form)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _auditService.LogAsync(adminId, "Modification paramètres", "Paramètres système modifiés (via interface)." );
            TempData["Success"] = "Paramètres sauvegardés (placeholder).";
            return RedirectToAction(nameof(Settings));
        }

        // ======================
        // Connections monitoring (UC8) - placeholder
        // ======================
        public IActionResult Connections()
        {
            var sessions = _context.Sessions
                .Where(s => s.EndTime == null)
                .OrderByDescending(s => s.StartTime)
                .ToList();

            return View(sessions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminateSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest();

            var session = _context.Sessions.Find(sessionId);
            if (session == null)
                return NotFound();

            session.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _auditService.LogAsync(adminId, "Forcer déconnexion", $"Session {sessionId} terminée.");

            TempData["Success"] = "Session terminée.";
            return RedirectToAction(nameof(Connections));
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Fichier requis.");
                return View();
            }

            using var stream = file.OpenReadStream();
            var (created, skipped, errors) = await _importService.ImportStudentsFromExcelAsync(stream);

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _auditService.LogAsync(adminId, "Import étudiants", $"Créés: {created}, Ignorés: {skipped}");

            TempData["Success"] = $"Import terminé. Créés: {created}. Ignorés: {skipped}.";
            if (errors.Any())
                TempData["Errors"] = string.Join("; ", errors);

            return RedirectToAction(nameof(Import));
        }
    }
}
