using EcoleApp.Models.Entity.Auth;
using EcoleApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = Roles.Enseignant)]
public class SeanceController : Controller
{
    private readonly ISeanceService _seanceService;

    public SeanceController(ISeanceService seanceService)
    {
        _seanceService = seanceService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var seances = await _seanceService.ConsulterSeancesAsync(userId);
        return View(seances);
    }

    // GET
    public IActionResult Create()
    {
        return View();
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSeanceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var enseignantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _seanceService.CreerSeanceAsync(
                model.CoursId,
                model.GroupeId,
                model.Date,
                model.Salle,
                model.Type,
                enseignantId);

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(int id)
    {
        try
        {
            var enseignantId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _seanceService.ValiderSeanceAsync(id, enseignantId);
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
