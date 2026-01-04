using EcoleApp.Models.DAL;
using EcoleApp.Models.Enums;
using EcoleApp.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class SeanceEditableHandler
    : AuthorizationHandler<SeanceEditableRequirement, int>
{
    private readonly ApplicationDbContext _context;

    public SeanceEditableHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SeanceEditableRequirement requirement,
        int seanceId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return;

        var seance = await _context.Seances
            .Include(s => s.Cours)
            .FirstOrDefaultAsync(s => s.Id == seanceId);

        if (seance == null) return;

        if (seance.Etat == EtatSeance.Brouillon &&
            seance.Cours!.EnseignantId == userId)
        {
            context.Succeed(requirement);
        }
    }
}
