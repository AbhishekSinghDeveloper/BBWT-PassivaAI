﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddTableSettingsToControlSetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabelColumnId",
                table: "RbWidgetControlSetItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TableId",
                table: "RbWidgetControlSetItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueColumnId",
                table: "RbWidgetControlSetItem",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabelColumnId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "ValueColumnId",
                table: "RbWidgetControlSetItem");
        }
    }
}
