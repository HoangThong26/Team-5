using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapstoneProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedScoresToDefense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_DefenseScores_DefenseId",
            //    table: "DefenseScores");

            migrationBuilder.AddColumn<decimal>(
                name: "DemoScore",
                table: "DefenseScores",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PresentationScore",
                table: "DefenseScores",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QAScore",
                table: "DefenseScores",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "UQ_Defense_CouncilMember",
                table: "DefenseScores",
                columns: new[] { "DefenseId", "CouncilMemberId" },
                unique: true,
                filter: "[DefenseId] IS NOT NULL AND [CouncilMemberId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Defense_CouncilMember",
                table: "DefenseScores");

            migrationBuilder.DropColumn(
                name: "DemoScore",
                table: "DefenseScores");

            migrationBuilder.DropColumn(
                name: "PresentationScore",
                table: "DefenseScores");

            migrationBuilder.DropColumn(
                name: "QAScore",
                table: "DefenseScores");

            migrationBuilder.CreateIndex(
                name: "IX_DefenseScores_DefenseId",
                table: "DefenseScores",
                column: "DefenseId");
        }
    }
}
