using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FrotaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolIdToOperationalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Schools_SchoolId",
                table: "Instructors");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Schools_SchoolId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Chassis",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_SchoolId",
                table: "Vehicles");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "VehicleDocuments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Students",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedLessonsCount",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentsSubmitted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExamScheduled",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PracticalLessonsStarted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProgressStatus",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RegistrationFeePaid",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RequiredLessonsCount",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TheoryCompleted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Maintenances",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Evaluation",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ExercisesCompletedJson",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrackingSessionId",
                table: "Lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Instructors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "FuelRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Accidents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "Accidents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Accidents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Accidents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SchoolId_Chassis",
                table: "Vehicles",
                columns: new[] { "SchoolId", "Chassis" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SchoolId_LicensePlate",
                table: "Vehicles",
                columns: new[] { "SchoolId", "LicensePlate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDocuments_SchoolId",
                table: "VehicleDocuments",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_SchoolId",
                table: "Maintenances",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_SchoolId",
                table: "Lessons",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelRecords_SchoolId",
                table: "FuelRecords",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Accidents_SchoolId",
                table: "Accidents",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accidents_Schools_SchoolId",
                table: "Accidents",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelRecords_Schools_SchoolId",
                table: "FuelRecords",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Schools_SchoolId",
                table: "Instructors",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Schools_SchoolId",
                table: "Lessons",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Schools_SchoolId",
                table: "Maintenances",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleDocuments_Schools_SchoolId",
                table: "VehicleDocuments",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Schools_SchoolId",
                table: "Vehicles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accidents_Schools_SchoolId",
                table: "Accidents");

            migrationBuilder.DropForeignKey(
                name: "FK_FuelRecords_Schools_SchoolId",
                table: "FuelRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Schools_SchoolId",
                table: "Instructors");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Schools_SchoolId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Schools_SchoolId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleDocuments_Schools_SchoolId",
                table: "VehicleDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Schools_SchoolId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_SchoolId_Chassis",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_SchoolId_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleDocuments_SchoolId",
                table: "VehicleDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_SchoolId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_SchoolId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_FuelRecords_SchoolId",
                table: "FuelRecords");

            migrationBuilder.DropIndex(
                name: "IX_Accidents_SchoolId",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "VehicleDocuments");

            migrationBuilder.DropColumn(
                name: "CompletedLessonsCount",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DocumentsSubmitted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ExamScheduled",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PracticalLessonsStarted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProgressStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RegistrationFeePaid",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RequiredLessonsCount",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TheoryCompleted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Evaluation",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ExercisesCompletedJson",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "TrackingSessionId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "FuelRecords");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Accidents");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Vehicles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Students",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolId",
                table: "Instructors",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Accidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "EstimatedCost",
                table: "Accidents",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Accidents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Chassis",
                table: "Vehicles",
                column: "Chassis",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SchoolId",
                table: "Vehicles",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Schools_SchoolId",
                table: "Instructors",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolId",
                table: "Students",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Schools_SchoolId",
                table: "Vehicles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");
        }
    }
}
