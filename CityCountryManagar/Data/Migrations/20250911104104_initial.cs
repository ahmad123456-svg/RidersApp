using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RidersApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Configurations",
                newName: "ConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfigurationId",
                table: "Configurations",
                newName: "Id");
        }
    }
}
