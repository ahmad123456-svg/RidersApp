using System;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class CityVM
    {
        public int CityId { get; set; }

        [Required]
        public string CityName { get; set; } = string.Empty;
        [Required]
        public string PostalCode { get; set; } = string.Empty;
        [Required]
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
    }
}
