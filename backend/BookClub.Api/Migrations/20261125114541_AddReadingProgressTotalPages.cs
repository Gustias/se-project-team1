using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingProgressTotalPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalPages",
                table: "ReadingProgresses",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPages",
                table: "ReadingProgresses");
        }
    }
}
