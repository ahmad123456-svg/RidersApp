using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RidersApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFineOrExpenseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FineOrExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    FineOrExpenseTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FineOrExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FineOrExpenses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FineOrExpenses_FineOrExpenseTypes_FineOrExpenseTypeId",
                        column: x => x.FineOrExpenseTypeId,
                        principalTable: "FineOrExpenseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FineOrExpenses_EmployeeId",
                table: "FineOrExpenses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FineOrExpenses_EntryDate",
                table: "FineOrExpenses",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_FineOrExpenses_FineOrExpenseTypeId",
                table: "FineOrExpenses",
                column: "FineOrExpenseTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FineOrExpenses");
        }
    }
}
