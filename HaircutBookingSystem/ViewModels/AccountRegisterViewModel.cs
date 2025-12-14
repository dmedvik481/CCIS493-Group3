using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.ViewModels
{
    public class AccountRegisterViewModel
    {
        [Required, MaxLength(256), EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required, MinLength(6), MaxLength(20)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
