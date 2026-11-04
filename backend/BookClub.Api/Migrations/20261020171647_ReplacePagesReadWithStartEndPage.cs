using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePagesReadWithStartEndPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingLogs_UserId_BookId_Date",
                table: "ReadingLogs");

            migrationBuilder.DropColumn(
                name: "PagesRead",
                table: "ReadingLogs");

            migrationBuilder.AddColumn<int>(
                name: "EndPage",
                table: "ReadingLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartPage",
                table: "ReadingLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingLogs_UserId_BookId_Date",
                table: "ReadingLogs",
                columns: new[] { "UserId", "BookId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingLogs_UserId_BookId_Date",
                table: "ReadingLogs");

            migrationBuilder.DropColumn(
                name: "EndPage",
                table: "ReadingLogs");

            migrationBuilder.DropColumn(
                name: "StartPage",
                table: "ReadingLogs");

            migrationBuilder.AddColumn<int>(
                name: "PagesRead",
                table: "ReadingLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingLogs_UserId_BookId_Date",
                table: "ReadingLogs",
                columns: new[] { "UserId", "BookId", "Date" },
                unique: true);
        }
    }
}
