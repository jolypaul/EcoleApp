using EcoleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoleApp.Controllers
{
    [Authorize(Roles = "Delegue")]
    public class DelegueController : Controller
    {
        private readonly ISeanceService _seanceService;

        public DelegueController(ISeanceService seanceService)
        {
            _seanceService = seanceService;
        }

        // =========================
        // DASHBOARD DELEGUE
        // =========================
        public async Task<IActionResult> Dashboard()
        {
            var delegueId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(delegueId))
                return Unauthorized();

            var seances = await _seanceService
                .ConsulterSeancesAsync(delegueId);

            // Séances du groupe du délégué
            var mySeances = seances
                .Where(s =>
                    s.Groupe != null &&
                    s.Groupe.Etudiants != null &&
                    s.Groupe.Etudiants.Any(e => e.Id == delegueId))
                .OrderByDescending(s => s.Date)
                .ToList();

            return View(mySeances);
        }
    }
}
