using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initiajlJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_LoanProducts_LoanProductId",
                table: "Requirements");

            migrationBuilder.AlterColumn<int>(
                name: "LoanProductId",
                table: "Requirements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_LoanProducts_LoanProductId",
                table: "Requirements",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_LoanProducts_LoanProductId",
                table: "Requirements");

            migrationBuilder.AlterColumn<int>(
                name: "LoanProductId",
                table: "Requirements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_LoanProducts_LoanProductId",
                table: "Requirements",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
