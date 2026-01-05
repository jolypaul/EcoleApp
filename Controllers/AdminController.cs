using EcoleApp.Models.DAL;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Services;
using EcoleApp.Utilitaires; 
using EcoleApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EcoleApp.Controllers
{
    [Authorize(Roles = Roles.Administrateur + "," + Roles.Responsable)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AdminService _adminService;
        private readonly AuditService _auditService;
        private readonly ImportService _importService;

        public AdminController(
            ApplicationDbContext db,
            AdminService adminService,
            AuditService auditService,
            ImportService importService)
        {
            _db = db;
            _adminService = adminService;
            _auditService = auditService;
            _importService = importService;
        }

        // =========================
        // DASHBOARD
        // =========================
        public IActionResult Dashboard()
        {
            return View();
        }

        // =========================
        // CREATE USER (UC1)
        // =========================
        [Authorize(Roles = Roles.Administrateur)]
        [HttpGet]
        public IActionResult CreateUser()
        {
            ChargerRoles();
            return View();
        }

        [Authorize(Roles = Roles.Administrateur)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ChargerRoles();
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            try
            {
                await _adminService.CreerUtilisateurAsync(
                    model.Nom,
                    model.Email,
                    model.MotDePasse,
                    model.RoleId,
                    adminId);

                await _auditService.LogAsync(
                    adminId,
                    "Création utilisateur",
                    $"Utilisateur {model.Email} créé");

                TempData["Success"] = "Utilisateur créé avec succès.";
                return RedirectToAction(nameof(CreateUser));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ChargerRoles();
                return View(model);
            }
        }

        // =========================
        // CHANGE ROLE (UC2)
        // =========================
        [Authorize(Roles = Roles.Administrateur)]
        [HttpGet]
        public IActionResult ChangeRole()
        {
            ViewBag.Utilisateurs =
                new SelectList(_db.Utilisateurs, "Id", "Email");
            ChargerRoles();
            return View();
        }

        [Authorize(Roles = Roles.Administrateur)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Utilisateurs =
                    new SelectList(_db.Utilisateurs, "Id", "Email");
                ChargerRoles();
                return View(model);
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            try
            {
                await _adminService.ChangerRoleAsync(
                    model.UtilisateurId,
                    model.NouveauRoleId,
                    adminId);

                await _auditService.LogAsync(
                    adminId,
                    "Changement rôle",
                    $"Rôle modifié pour l'utilisateur {model.UtilisateurId}");

                TempData["Success"] = "Rôle modifié avec succès.";
                return RedirectToAction(nameof(ChangeRole));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Utilisateurs =
                    new SelectList(_db.Utilisateurs, "Id", "Email");
                ChargerRoles();
                return View(model);
            }
        }

        // =========================
        // ROLES MANAGEMENT (UC5)
        // =========================
        [Authorize(Roles = Roles.Administrateur)]
        public IActionResult ManageRoles() // ✅ renommé
        {
            return View(_db.Roles.ToList());
        }

        [Authorize(Roles = Roles.Administrateur)]
        [HttpGet]
        public IActionResult EditRole(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var role = _db.Roles.Find(id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        [Authorize(Roles = Roles.Administrateur)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(Role model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = _db.Roles.Find(model.Id);
            if (role == null)
                return NotFound();

            role.NomRole = model.NomRole;
            role.Responsabilite = model.Responsabilite;
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                "Modification rôle",
                $"Rôle {role.NomRole} modifié");

            TempData["Success"] = "Rôle mis à jour.";
            return RedirectToAction(nameof(ManageRoles));
        }

        // =========================
        // CONNECTIONS MONITORING (UC8)
        // =========================
        [Authorize(Roles = Roles.Administrateur)]
        public IActionResult Connections()
        {
            var sessions = _db.Sessions
                .Where(s => s.EndTime == null)
                .OrderByDescending(s => s.StartTime)
                .ToList();

            return View(sessions);
        }

        [Authorize(Roles = Roles.Administrateur)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminateSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest();

            var session = _db.Sessions.Find(sessionId);
            if (session == null)
                return NotFound();

            session.EndTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _auditService.LogAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                "Forcer déconnexion",
                $"Session {sessionId} terminée");

            TempData["Success"] = "Session terminée.";
            return RedirectToAction(nameof(Connections));
        }

        // =========================
        // IMPORT ETUDIANTS
        // =========================
        [Authorize(Roles = Roles.Administrateur)]
        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [Authorize(Roles = Roles.Administrateur)]
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
            var (created, skipped, errors) =
                await _importService.ImportStudentsFromExcelAsync(stream);

            await _auditService.LogAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                "Import étudiants",
                $"Créés: {created}, Ignorés: {skipped}");

            TempData["Success"] =
                $"Import terminé. Créés: {created}. Ignorés: {skipped}.";

            if (errors.Any())
                TempData["Errors"] = string.Join(" | ", errors);

            return RedirectToAction(nameof(Import));
        }

        // =========================
        // OUTILS PRIVES
        // =========================
        private void ChargerRoles()
        {
            ViewBag.Roles =
                new SelectList(_db.Roles, "Id", "NomRole");
        }
    }
}
