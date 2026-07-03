using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_PaymentModalities_PaymentModalityId",
                table: "LoanApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_PaymentModalities_PaymentModalityId",
                table: "LoanApplications",
                column: "PaymentModalityId",
                principalTable: "PaymentModalities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_PaymentModalities_PaymentModalityId",
                table: "LoanApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_PaymentModalities_PaymentModalityId",
                table: "LoanApplications",
                column: "PaymentModalityId",
                principalTable: "PaymentModalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
