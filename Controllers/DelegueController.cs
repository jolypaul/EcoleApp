using EcoleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var seances = await _seanceService.ConsulterSeancesAsync(userId);

            // show only seances where user is student/delegate (group member)
            var mySeances = seances.Where(s => s.Groupe != null && s.Groupe.Etudiants != null && s.Groupe.Etudiants.Any(e => e.Id == userId))
                                   .OrderByDescending(s => s.Date)
                                   .ToList();

            return View(mySeances);
        }
    }
}
