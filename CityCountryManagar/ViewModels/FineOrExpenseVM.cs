using System;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class FineOrExpenseVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an employee")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Fine/Expense Type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a fine/expense type")]
        [Display(Name = "Fine/Expense Type")]
        public int FineOrExpenseTypeId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entry Date is required")]
        [Display(Name = "Entry Date")]
        public DateTime EntryDate { get; set; } = DateTime.Now;

        // Display properties for showing in views
        [Display(Name = "Employee")]
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "Type")]
        public string FineOrExpenseTypeName { get; set; } = string.Empty;
    }
}