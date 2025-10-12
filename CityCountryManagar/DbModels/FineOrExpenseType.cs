using System.ComponentModel.DataAnnotations;

namespace RidersApp.DbModels
{
    public class FineOrExpenseType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}