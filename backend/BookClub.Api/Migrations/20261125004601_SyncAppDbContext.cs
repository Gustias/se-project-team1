using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncAppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PagesRead",
                table: "ReadingProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagesRead",
                table: "ReadingProgresses");
        }
    }
}
