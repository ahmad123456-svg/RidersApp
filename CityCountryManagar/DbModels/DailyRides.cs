using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RidersApp.DbModels
{
    public class DailyRides
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditWAT { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashWAT { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Expense { get; set; }
        [DataType(DataType.Date)]
        public DateTime EntryDate { get; set; }
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }  // Navigation property
        public int TodayRides { get; set; }
        public int OverRides { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OverRidesAmount { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime InsertDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? UpdateDate { get; set; }
        public string InsertedBy { get; set; } = "System";
        public string UpdatedBy { get; set; } = "System";
        public int TotalRides { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LessAmount { get; set; }
    }
}
