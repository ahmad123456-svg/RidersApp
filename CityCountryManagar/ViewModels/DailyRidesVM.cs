using System;
using System.ComponentModel.DataAnnotations;

namespace RidersApp.ViewModels
{
    public class DailyRidesVM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal CreditAmount { get; set; }

        [Required]
        public decimal CreditWAT { get; set; }

        [Required]
        public decimal CashAmount { get; set; }

        [Required]
        public decimal CashWAT { get; set; }

        [Required]
        public decimal Expense { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EntryDate { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        [Required]
        public int TodayRides { get; set; }

        [Required]
        public int OverRides { get; set; }

        [Required]
        public decimal OverRidesAmount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime InsertDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UpdateDate { get; set; }

        public string InsertedBy { get; set; }

        public string UpdatedBy { get; set; }

        [Required]
        public int TotalRides { get; set; }

        [Required]
        public decimal LessAmount { get; set; }
    }
}
