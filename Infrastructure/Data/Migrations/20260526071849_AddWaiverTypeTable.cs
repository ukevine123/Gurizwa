using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWaiverTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WaiverType",
                table: "Waivers",
                newName: "WaiverTypeName");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "Waivers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "WaiverTypeId",
                table: "Waivers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WaiverTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaiverTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanProductId = table.Column<int>(type: "int", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaiverTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaiverTypes_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalTable: "LoanProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Waivers_WaiverTypeId",
                table: "Waivers",
                column: "WaiverTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WaiverTypes_LoanProductId",
                table: "WaiverTypes",
                column: "LoanProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Waivers_WaiverTypes_WaiverTypeId",
                table: "Waivers",
                column: "WaiverTypeId",
                principalTable: "WaiverTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Waivers_WaiverTypes_WaiverTypeId",
                table: "Waivers");

            migrationBuilder.DropTable(
                name: "WaiverTypes");

            migrationBuilder.DropIndex(
                name: "IX_Waivers_WaiverTypeId",
                table: "Waivers");

            migrationBuilder.DropColumn(
                name: "WaiverTypeId",
                table: "Waivers");

            migrationBuilder.RenameColumn(
                name: "WaiverTypeName",
                table: "Waivers",
                newName: "WaiverType");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                table: "Waivers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
