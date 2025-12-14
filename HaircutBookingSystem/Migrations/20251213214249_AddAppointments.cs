using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaircutBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stylists_AspNetUsers_UserId",
                table: "Stylists");

            migrationBuilder.DropIndex(
                name: "IX_Stylists_UserId",
                table: "Stylists");

            migrationBuilder.RenameColumn(
                name: "StylistId",
                table: "Stylists",
                newName: "StylistID");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Stylists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Stylists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Stylists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StylistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Stylists_StylistId",
                        column: x => x.StylistId,
                        principalTable: "Stylists",
                        principalColumn: "StylistID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StylistId_StartTime",
                table: "Appointments",
                columns: new[] { "StylistId", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Stylists");

            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Stylists");

            migrationBuilder.RenameColumn(
                name: "StylistID",
                table: "Stylists",
                newName: "StylistId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Stylists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stylists_UserId",
                table: "Stylists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stylists_AspNetUsers_UserId",
                table: "Stylists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
