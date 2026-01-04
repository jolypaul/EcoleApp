using EcoleApp.Models.Entity.Auth;
using EcoleApp.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

public class AbsenceConsultationHandler
    : AuthorizationHandler<AbsenceConsultationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AbsenceConsultationRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Responsable))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
