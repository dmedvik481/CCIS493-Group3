using System;
using System.Collections.Generic;
using System.Linq;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HaircutBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private RoleManager<IdentityRole> roleManager;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.db = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(AccountRegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email };
                var result = await userManager.CreateAsync(user, vm.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(vm);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(vm.Email, vm.Password, false, false);
                if (result.Succeeded)
                {
                    var user = await userManager.FindByEmailAsync(vm.Email);
                    var roles = await userManager.GetRolesAsync(user);
                    if (roles.Count > 1)
                    {
                        HttpContext.Session.SetString("UserRoles", JsonConvert.SerializeObject(roles));
                        return RedirectToAction("SelectRole");
                    }
                    else if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Manage", "Admin");
                    }
                    else if (roles.Contains("Stylist"))
                    {
                        return RedirectToAction("Index", "Stylist");
                    }
                    
                        return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Login Failure.");
            }
            return View(vm);
        }

       [Authorize(Roles = "Admin")]
        public IActionResult AllUser()
        {
            var users = db.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = userManager.GetRolesAsync(user).Result.ToList();
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public IActionResult SelectRole()
        {
            var roles = HttpContext.Session.GetString("UserRoles");
            var roleList = JsonConvert.DeserializeObject<List<string>>(roles);
            return View(roleList);
        }

        [HttpPost]
        public IActionResult SetRole(string SelectedRole)
        {
            if (SelectedRole == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (SelectedRole == "Instructor")
            {
                return RedirectToAction("Index", "Instructor");
            }
            else if (SelectedRole == "Student")
            {
                return RedirectToAction("Index", "Student");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
