using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaircutBookingSystem.ViewModels
{
    public class RoleAddUserRoleViewModel
    {
        public ApplicationUser? User { get; set; }          // used when id is provided
        public string? SelectedUserId { get; set; }         // used when choosing from list
        public List<SelectListItem> UserList { get; set; } = new();

        [Required]
        public string Role { get; set; } = string.Empty;

        public SelectList RoleList { get; set; } = new SelectList(new List<string>());
    }
}
