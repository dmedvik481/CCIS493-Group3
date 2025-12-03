using Microsoft.AspNetCore.Mvc;

namespace HaircutBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        // Main landing page
        public IActionResult Index()
        {
            return View();
        }

        // Privacy page (optional, for nav/footer links)
        public IActionResult Privacy()
        {
            return View();
        }
    }
}