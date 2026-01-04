using EcoleApp.Models.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EcoleApp.Middlewares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            try
            {
                // Only validate authenticated users
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    // don't validate for the login page to avoid redirect loop
                    var path = context.Request.Path.Value ?? string.Empty;
                    if (!path.StartsWith("/Account/Login", System.StringComparison.OrdinalIgnoreCase)
                        && !path.StartsWith("/Account/Logout", System.StringComparison.OrdinalIgnoreCase)
                        && !path.StartsWith("/css", System.StringComparison.OrdinalIgnoreCase)
                        && !path.StartsWith("/js", System.StringComparison.OrdinalIgnoreCase))
                    {
                        var sessionId = context.User.FindFirst("SessionId")?.Value;
                        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(userId))
                        {
                            // no session claim -> sign out
                            await context.SignOutAsync();
                            context.Response.Redirect("/Account/Login");
                            return;
                        }

                        var session = await db.Sessions.FindAsync(sessionId);
                        if (session == null || session.UtilisateurId != userId || session.EndTime != null)
                        {
                            await context.SignOutAsync();
                            context.Response.Redirect("/Account/Login");
                            return;
                        }
                    }
                }
            }
            catch
            {
                // On error, sign out to be safe
                try { await context.SignOutAsync(); } catch { }
                context.Response.Redirect("/Account/Login");
                return;
            }

            await _next(context);
        }
    }
}
