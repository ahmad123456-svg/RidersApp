using System.ComponentModel.DataAnnotations;

namespace RidersApp.DbModels
{
    public class Configuration
    {
        [Key]
        public int ConfigurationId { get; set; }

        [Required]
        [StringLength(100)]
        public string KeyName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;
    }
}
