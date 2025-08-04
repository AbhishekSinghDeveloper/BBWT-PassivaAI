using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddCreatedOnToSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "RbWidgetSource",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "RbQuerySource",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "RbWidgetSource");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "RbQuerySource");
        }
    }
}
