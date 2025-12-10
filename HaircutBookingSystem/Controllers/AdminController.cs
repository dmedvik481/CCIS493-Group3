using HaircutBookingSystem.Models;
using HaircutBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HaircutBookingSystem.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Manage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AllRole()
        {
            return RedirectToAction("AllRole", "Role");
        }

        [HttpGet]
        public async Task<IActionResult> AllUser()
        {
            return RedirectToAction("AllUser", "Account");
        }
    }
}
