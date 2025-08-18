using System;
using Microsoft.AspNetCore.Identity;

namespace RidersApp.Areas.Identity.Data
{
    public class RidersAppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Optional: Store role directly in the user table for easier access
        public string Role { get; set; }
    }
}
