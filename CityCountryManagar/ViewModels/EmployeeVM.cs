using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RidersApp.ViewModels
{
    public class EmployeeVM
    {
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string FatherName { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNo { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        public string CountryName { get; set; } = string.Empty;

        public string CityName { get; set; } = string.Empty;

        public decimal Salary { get; set; }
        [StringLength(20)]
        public string Vehicle { get; set; } = string.Empty;
        [StringLength(30)]
        public string VehicleNumber { get; set; } = string.Empty;
        
        // Picture related properties
        public string? Picture { get; set; } // Current picture path
        public IFormFile? PictureFile { get; set; } // File upload field
        public string PictureUrl => !string.IsNullOrEmpty(Picture) ? Picture : "/Image/download.png"; // Default image fallback
    }
}
