using EcoleApp.Models.DAL;
using EcoleApp.Services;
using EcoleApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcoleApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminService _adminService;

        public AdminController(
            ApplicationDbContext context,
            AdminService adminService)
        {
            _context = context;
            _adminService = adminService;
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

            var adminId = User.FindFirst("UserId")?.Value!;

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

            var adminId = User.FindFirst("UserId")?.Value!;

            await _adminService.ChangerRoleAsync(
                model.UtilisateurId,
                model.NouveauRoleId,
                adminId
            );

            TempData["Success"] = "Rôle modifié avec succès.";
            return RedirectToAction(nameof(ChangeRole));
        }


    }
}
