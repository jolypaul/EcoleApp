using EcoleApp.Models.Entity.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EcoleApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity!.IsAuthenticated)
                return View();

            if (User.IsInRole(Roles.Admin))
                return RedirectToAction("Dashboard", "Admin");

            if (User.IsInRole(Roles.Enseignant))
                return RedirectToAction("Dashboard", "Responsable");

            if (User.IsInRole(Roles.Delegue))
                return RedirectToAction("Dashboard", "Delegue");

            return View();
        }
    }
}
