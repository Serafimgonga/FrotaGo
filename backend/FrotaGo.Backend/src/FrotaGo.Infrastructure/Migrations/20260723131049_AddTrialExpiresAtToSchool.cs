using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrotaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialExpiresAtToSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TrialExpiresAt",
                table: "Schools",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrialExpiresAt",
                table: "Schools");
        }
    }
}
