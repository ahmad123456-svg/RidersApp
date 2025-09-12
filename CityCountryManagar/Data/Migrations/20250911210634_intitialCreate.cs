using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RidersApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class intitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Vehicle",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleNumber",
                table: "Employees",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Vehicle",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "VehicleNumber",
                table: "Employees");
        }
    }
}
