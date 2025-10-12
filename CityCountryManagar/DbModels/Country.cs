using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.DbModels
{
    public class Country
    {
        [Key]
        public int CountryId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public ICollection<City> Cities { get; set; } = new List<City>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
