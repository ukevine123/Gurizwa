using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initiail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Borrowers");

            migrationBuilder.AddColumn<string>(
                name: "Cell",
                table: "Guarantors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Guarantors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Guarantors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Guarantors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Guarantors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cell",
                table: "Borrowers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Borrowers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Borrowers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Borrowers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Borrowers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cell",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "Cell",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Borrowers");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Borrowers");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Guarantors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Borrowers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
