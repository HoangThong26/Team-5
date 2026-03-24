using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    public partial class AddGithubAndFileToWeeklyReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GithubLink",
                table: "WeeklyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "WeeklyReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "GithubLink", table: "WeeklyReports");
            migrationBuilder.DropColumn(name: "FileUrl", table: "WeeklyReports");
        }
    }
}