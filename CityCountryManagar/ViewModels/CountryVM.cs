using System;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class CountryVM
    {
        public int CountryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
