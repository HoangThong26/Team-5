using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterAndMajor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "WeekDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MajorId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MajorId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPass",
                table: "FinalGrades",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    MajorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MajorName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.MajorId);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    SemesterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SemesterName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.SemesterId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeekDefinitions_SemesterId",
                table: "WeekDefinitions",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MajorId",
                table: "Users",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_MajorId",
                table: "Groups",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SemesterId",
                table: "Groups",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_MajorCode",
                table: "Majors",
                column: "MajorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_SemesterCode",
                table: "Semesters",
                column: "SemesterCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Majors_MajorId",
                table: "Groups",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Semesters_SemesterId",
                table: "Groups",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Majors_MajorId",
                table: "Users",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeekDefinitions_Semesters_SemesterId",
                table: "WeekDefinitions",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Majors_MajorId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Semesters_SemesterId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Majors_MajorId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WeekDefinitions_Semesters_SemesterId",
                table: "WeekDefinitions");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_WeekDefinitions_SemesterId",
                table: "WeekDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Users_MajorId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Groups_MajorId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_SemesterId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "WeekDefinitions");

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsPass",
                table: "FinalGrades");
        }
    }
}
