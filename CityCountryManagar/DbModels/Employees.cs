using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RidersApp.DbModels
{
    public class Employee
    {
        [Key]
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
        // Foreign Key for Country
        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public Country Country { get; set; }
        // Foreign Key for City
        [ForeignKey("City")]
        public int CityId { get; set; }
        public City City { get; set; }
        // Navigation for related DailyRides
        public ICollection<DailyRides> DailyRides { get; set; } = new List<DailyRides>();
        public decimal Salary { get; set; }
        [StringLength(20)]
        public string Vehicle { get; set; }
        [StringLength(30)]
        public string VehicleNumber { get; set; }
    }
}
