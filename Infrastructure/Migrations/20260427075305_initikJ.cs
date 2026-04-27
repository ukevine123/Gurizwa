using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initikJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanProducts_LoanProductId",
                table: "LoanApplications");

            migrationBuilder.RenameColumn(
                name: "LoanProductId",
                table: "LoanApplications",
                newName: "LoanProductSettingId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplications_LoanProductId",
                table: "LoanApplications",
                newName: "IX_LoanApplications_LoanProductSettingId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanProductSettings_LoanProductSettingId",
                table: "LoanApplications",
                column: "LoanProductSettingId",
                principalTable: "LoanProductSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_LoanProductSettings_LoanProductSettingId",
                table: "LoanApplications");

            migrationBuilder.RenameColumn(
                name: "LoanProductSettingId",
                table: "LoanApplications",
                newName: "LoanProductId");

            migrationBuilder.RenameIndex(
                name: "IX_LoanApplications_LoanProductSettingId",
                table: "LoanApplications",
                newName: "IX_LoanApplications_LoanProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_LoanProducts_LoanProductId",
                table: "LoanApplications",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
