using System.ComponentModel.DataAnnotations;

namespace RidersApp.DbModels
{
    public class Configuration
    {
        [Key]
        public int ConfigurationId { get; set; }

        [Required]
        [StringLength(200)]
        public string KeyName { get; set; }

        [StringLength(2000)]
        public string Value { get; set; }
    }
}
