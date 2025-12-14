using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace HaircutBookingSystem.Controllers
{
    [Authorize(Roles = "Stylist")]
    public class StylistController : Controller
    {
        private readonly ApplicationDbContext db;

        public StylistController(ApplicationDbContext db)
        {
            this.db = db;
        }
        
        public IActionResult Index()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewData["Stylist"] = db.Stylists.FirstOrDefault(s => s.UserId == currentUserId);
            return View();
        }
        public IActionResult AllStylist()
        {
            return View(db.Stylists);
        }

        public IActionResult AddProfile()
        { 
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Stylist stylist = new Stylist();

            if (db.Stylists.Any(s => s.UserId == currentUserId))
            {
                stylist = db.Stylists.FirstOrDefault(s => s.UserId == currentUserId);
            }
            else
            {
                stylist.UserId = currentUserId;
            }
            return View(stylist);
        }

        [HttpPost]
        public async Task<IActionResult> AddProfile(Stylist stylist)
        {
            var CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (db.Stylists.Any(s => s.UserId == CurrentUserId))
            {
                var stylistToUpdate = db.Stylists.FirstOrDefault(s => s.UserId == CurrentUserId);
                stylistToUpdate.StylistName = stylist.StylistName;
                db.Update(stylistToUpdate);
            }
            else
            {
                db.Add(stylist);
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
