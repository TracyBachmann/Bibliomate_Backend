using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendBiblioMate.Migrations
{
    /// <inheritdoc />
    public partial class AddExtensionsCountToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtensionsCount",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtensionsCount",
                table: "Loans");
        }
    }
}
