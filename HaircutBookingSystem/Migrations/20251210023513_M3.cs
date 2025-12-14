using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaircutBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class M3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Stylists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stylists_UserId",
                table: "Stylists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stylists_AspNetUsers_UserId",
                table: "Stylists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stylists_AspNetUsers_UserId",
                table: "Stylists");

            migrationBuilder.DropIndex(
                name: "IX_Stylists_UserId",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Stylists");
        }
    }
}
