using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class UserVM
    {
        // Primary fields (matching your current implementation)
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Role { get; set; }

        // Password field (for create/edit only - not stored directly)
        public string Password { get; set; }

        // Additional database fields (optional for display/advanced management)
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        // Security fields (read-only for display purposes)
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }

        // Computed properties
        public bool IsEdit => !string.IsNullOrEmpty(Id);
        public bool HasExistingPassword => IsEdit;
        public string FullName => $"{FirstName} {LastName}";
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.Now;
        public string UserStatus => IsLocked ? "Locked" : EmailConfirmed ? "Active" : "Pending";
    }
}
