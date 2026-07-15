using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrotaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrackingSessionId",
                table: "VehicleLocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingSessions_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrackingSessions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrackingSessions_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocations_TrackingSessionId",
                table: "VehicleLocations",
                column: "TrackingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingSessions_InstructorId",
                table: "TrackingSessions",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingSessions_LessonId",
                table: "TrackingSessions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingSessions_VehicleId",
                table: "TrackingSessions",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_TrackingSessions_TrackingSessionId",
                table: "VehicleLocations",
                column: "TrackingSessionId",
                principalTable: "TrackingSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_TrackingSessions_TrackingSessionId",
                table: "VehicleLocations");

            migrationBuilder.DropTable(
                name: "TrackingSessions");

            migrationBuilder.DropIndex(
                name: "IX_VehicleLocations_TrackingSessionId",
                table: "VehicleLocations");

            migrationBuilder.DropColumn(
                name: "TrackingSessionId",
                table: "VehicleLocations");
        }
    }
}
