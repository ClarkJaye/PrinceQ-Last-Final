using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrinceQ.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ForFilling_start_Backup",
                table: "QueueNumbers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Releasing_start_Backup",
                table: "QueueNumbers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForFilling_start_Backup",
                table: "QueueNumbers");

            migrationBuilder.DropColumn(
                name: "Releasing_start_Backup",
                table: "QueueNumbers");
        }
    }
}
