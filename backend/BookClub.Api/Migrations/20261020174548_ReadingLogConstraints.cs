using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReadingLogConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_ReadingLogs_Pages",
                table: "ReadingLogs",
                sql: "\"EndPage\" >= \"StartPage\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ReadingLogs_Pages",
                table: "ReadingLogs");
        }
    }
}
