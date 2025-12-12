using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaircutBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager,
                              UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // ===== Roles list =====
        [HttpGet]
        public async Task<IActionResult> AllRole()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();

            ViewBag.OkMessage = TempData["Ok"] as string;
            ViewBag.ErrMessage = TempData["Error"] as string;

            return View(roles); // Views/Role/AllRole.cshtml
        }

        // ===== (optional) create role =====
        [HttpGet]
        public IActionResult AddRole() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError(string.Empty, "Role name is required.");
                return View();
            }

            if (!await _roleManager.RoleExistsAsync(name))
                await _roleManager.CreateAsync(new IdentityRole(name));

            return RedirectToAction(nameof(AllRole));
        }

        // ===== Delete role (guarded) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // guardrails
            if (string.Equals(role.Name, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "The Admin role cannot be deleted.";
                return RedirectToAction(nameof(AllRole));
            }

            var inRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (inRole.Any())
            {
                TempData["Error"] = $"Cannot delete '{role.Name}' — {inRole.Count} user(s) still have this role.";
                return RedirectToAction(nameof(AllRole));
            }

            var result = await _roleManager.DeleteAsync(role);
            TempData[result.Succeeded ? "Ok" : "Error"] =
                result.Succeeded ? "Role deleted." : string.Join("; ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(AllRole));
        }

        // ===== Assign role to user =====
        // GET: /Role/AddUserRole[?id=<userId>]
        [HttpGet]
        public async Task<IActionResult> AddUserRole(string? id)
        {
            var vm = new RoleAddUserRoleViewModel();

            var roleNames = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(n => n)
                .ToListAsync();
            vm.RoleList = new SelectList(roleNames);

            if (!string.IsNullOrWhiteSpace(id))
            {
                vm.User = await _userManager.FindByIdAsync(id);
                if (vm.User == null) return NotFound();
            }
            else
            {
                vm.UserList = await _userManager.Users
                    .OrderBy(u => u.Email)
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                    .ToListAsync();
            }

            return View(vm); // Views/Role/AddUserRole.cshtml
        }

        // POST: /Role/AddUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserRole(RoleAddUserRoleViewModel vm)
        {
            // Resolve user
            var user = vm.User;
            if (user == null && !string.IsNullOrWhiteSpace(vm.SelectedUserId))
                user = await _userManager.FindByIdAsync(vm.SelectedUserId);

            if (user == null)
                ModelState.AddModelError(string.Empty, "Please select a user.");

            // Validate role
            if (string.IsNullOrWhiteSpace(vm.Role) || !await _roleManager.RoleExistsAsync(vm.Role))
                ModelState.AddModelError(string.Empty, "Please select a valid role.");

            if (!ModelState.IsValid)
            {
                var roleNames = await _roleManager.Roles
                    .Select(r => r.Name!)
                    .OrderBy(n => n)
                    .ToListAsync();
                vm.RoleList = new SelectList(roleNames);

                if (vm.User == null)
                {
                    vm.UserList = await _userManager.Users
                        .OrderBy(u => u.Email)
                        .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                        .ToListAsync();
                }
                return View(vm);
            }

            var result = await _userManager.AddToRoleAsync(user!, vm.Role);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                var roleNames = await _roleManager.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
                vm.RoleList = new SelectList(roleNames);
                return View(vm);
            }

            return RedirectToAction("AllUser", "Account");
        }
    }
}
