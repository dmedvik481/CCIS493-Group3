using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaircutBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUnavailableSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnavailableSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StylistId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnavailableSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnavailableSlots_Stylists_StylistId",
                        column: x => x.StylistId,
                        principalTable: "Stylists",
                        principalColumn: "StylistID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnavailableSlots_StylistId_StartTime",
                table: "UnavailableSlots",
                columns: new[] { "StylistId", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnavailableSlots");
        }
    }
}
