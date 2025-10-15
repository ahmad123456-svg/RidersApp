using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RidersApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePictureColumnToPicturePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Picture",
                table: "Employees",
                newName: "PicturePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PicturePath",
                table: "Employees",
                newName: "Picture");
        }
    }
}
