using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateRead",
                table: "ReadingProgresses",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ReadingProgresses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Reading");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReadingProgresses");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateRead",
                table: "ReadingProgresses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
