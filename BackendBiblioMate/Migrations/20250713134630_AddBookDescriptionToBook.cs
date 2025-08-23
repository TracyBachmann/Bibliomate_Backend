using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendBiblioMate.Migrations
{
    /// <inheritdoc />
    public partial class AddBookDescriptionToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Stocks");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Books");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Stocks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
