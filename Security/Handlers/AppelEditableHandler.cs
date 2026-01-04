using EcoleApp.Models.DAL;
using EcoleApp.Models.Enums;
using EcoleApp.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AppelEditableHandler
    : AuthorizationHandler<AppelEditableRequirement, int>
{
    private readonly ApplicationDbContext _context;

    public AppelEditableHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AppelEditableRequirement requirement,
        int appelId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return;

        var appel = await _context.Appels
            .Include(a => a.Seance)
            .FirstOrDefaultAsync(a => a.Id == appelId);

        if (appel == null) return;

        if (!appel.EstVerrouille &&
            appel.Seance!.Etat == EtatSeance.Brouillon &&
            appel.DelegueId == userId)
        {
            context.Succeed(requirement);
        }
    }
}
