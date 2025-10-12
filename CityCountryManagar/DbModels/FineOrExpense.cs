using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RidersApp.DbModels
{
    public class FineOrExpense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int FineOrExpenseTypeId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EntryDate { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("FineOrExpenseTypeId")]
        public virtual FineOrExpenseType FineOrExpenseType { get; set; } = null!;
    }
}