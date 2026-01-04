using EcoleApp.Exceptions;
using EcoleApp.Models.Entity.Auth;
using EcoleApp.Models.Entity.GestionAppel;
using EcoleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoleApp.Controllers
{
    [Authorize]
    public class AppelController : Controller
    {
        private readonly IAppelService _appelService;
        private readonly IExportAbsenceService _exportAbsenceService;

        public AppelController(
            IAppelService appelService,
            IExportAbsenceService exportAbsenceService)
        {
            _appelService = appelService;
            _exportAbsenceService = exportAbsenceService;
        }

        // =========================
        // CRÉATION DE L’APPEL (GET)
        // =========================
        [Authorize(Roles = Roles.Delegue)]
        public async Task<IActionResult> Creer(int seanceId)
        {
            if (seanceId <= 0)
                return BadRequest();

            try
            {
                var delegueId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(delegueId))
                    return Unauthorized();

                var appelId = await _appelService.CreerAppelAsync(seanceId, delegueId);
                return RedirectToAction(nameof(Edit), new { appelId });
            }
            catch (BusinessException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Seance");
            }
        }

        // =========================
        // SAISIE DE L’APPEL (GET)
        // =========================
        [Authorize(Roles = Roles.Delegue)]
        public async Task<IActionResult> Edit(int appelId)
        {
            if (appelId <= 0)
                return BadRequest();

            var model = await _appelService.GetSaisieAppelAsync(appelId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Roles.Delegue)]
        public async Task<IActionResult> Edit(int appelId, [FromForm] Dictionary<string, int> presences)
        {
            if (appelId <= 0)
                return BadRequest();

            // Convert ints to StatutPresence enum
            var dict = presences.ToDictionary(kv => kv.Key, kv => (EcoleApp.Models.Enums.StatutPresence)kv.Value);

            try
            {
                await _appelService.EnregistrerSaisieAsync(appelId, dict);
                TempData["Success"] = "Appel enregistré.";
                return RedirectToAction("Index", "Seance");
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // reload view model
                var model = await _appelService.GetSaisieAppelAsync(appelId);
                return View(model);
            }
        }

        // =========================
        // CONSULTATION DES ABSENCES
        // =========================
        [Authorize(Roles = Roles.Responsable)]
        public async Task<IActionResult> Absences(DateTime debut, DateTime fin)
        {
            if (debut > fin)
            {
                ModelState.AddModelError("", "La date de début doit être antérieure à la date de fin.");
                return View();
            }

            var absences = await _appelService.ConsulterAbsencesAsync(debut, fin);
            return View(absences);
        }

        // =========================
        // EXPORT EXCEL
        // =========================
        [Authorize(Policy = "AbsenceConsultation")]
        public async Task<IActionResult> ExportExcel(DateTime debut, DateTime fin)
        {
            if (debut > fin)
                return BadRequest();

            var bytes = await _exportAbsenceService.ExportExcelAsync(debut, fin);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "absences.xlsx");
        }

        // =========================
        // EXPORT PDF
        // =========================
        [Authorize(Policy = "AbsenceConsultation")]
        public async Task<IActionResult> ExportPdf(DateTime debut, DateTime fin)
        {
            if (debut > fin)
                return BadRequest();

            var bytes = await _exportAbsenceService.ExportPdfAsync(debut, fin);

            return File(bytes, "application/pdf", "absences.pdf");
        }
    }
}
