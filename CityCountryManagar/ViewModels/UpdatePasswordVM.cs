using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class UpdatePasswordVM
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}