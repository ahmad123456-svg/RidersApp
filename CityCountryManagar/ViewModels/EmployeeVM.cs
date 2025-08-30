using System;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class EmployeeVM
    {
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string FatherName { get; set; }

        [StringLength(20)]
        public string PhoneNo { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int CityId { get; set; }

        public string CountryName { get; set; }

        public string CityName { get; set; }
    }
}
