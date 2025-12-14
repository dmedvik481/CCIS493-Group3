using Microsoft.AspNetCore.Mvc;

namespace HaircutBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Services() => View();   // list services / pricing
        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
}