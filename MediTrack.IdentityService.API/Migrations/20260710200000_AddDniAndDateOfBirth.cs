using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediTrack.IdentityService.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDniAndDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "users",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dni",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "users");
        }
    }
}
