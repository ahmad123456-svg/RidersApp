using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class UserVM
    {
        // Primary fields (matching your current implementation)
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // UserName will be set to Email automatically - not displayed in form
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        // Password field (for create/edit only - not stored directly)
        public string Password { get; set; } = string.Empty;

        // Confirm password field for validation
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Additional database fields (optional for display/advanced management)
        public string NormalizedUserName { get; set; } = string.Empty;
        public string NormalizedEmail { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        // Security fields (read-only for display purposes)
        public string SecurityStamp { get; set; } = string.Empty;
        public string ConcurrencyStamp { get; set; } = string.Empty;

        // Computed properties
        public bool IsEdit => !string.IsNullOrEmpty(Id);
        public bool HasExistingPassword => IsEdit;
        public string FullName => $"{FirstName} {LastName}";
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.Now;
        public string UserStatus => IsLocked ? "Locked" : EmailConfirmed ? "Active" : "Pending";
    }
}
