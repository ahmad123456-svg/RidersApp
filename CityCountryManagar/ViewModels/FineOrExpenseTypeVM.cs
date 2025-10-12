using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class FineOrExpenseTypeVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "Type Name")]
        public string Name { get; set; } = string.Empty;
    }
}