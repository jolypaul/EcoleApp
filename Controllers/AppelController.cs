using Microsoft.AspNetCore.Mvc;

namespace EcoleApp.Controllers
{
    public class AppelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
