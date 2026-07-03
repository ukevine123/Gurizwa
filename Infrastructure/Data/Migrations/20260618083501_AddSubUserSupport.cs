using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubUserSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentUserId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ParentUserId",
                table: "Users",
                column: "ParentUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ParentUserId",
                table: "Users",
                column: "ParentUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ParentUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ParentUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ParentUserId",
                table: "Users");
        }
    }
}
