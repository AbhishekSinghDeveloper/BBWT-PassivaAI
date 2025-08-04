using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddWidgetDrafting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "RbWidgetSource",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseWidgetId",
                table: "RbWidgetSource",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetSource_ReleaseWidgetId",
                table: "RbWidgetSource",
                column: "ReleaseWidgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetSource_RbWidgetSource_ReleaseWidgetId",
                table: "RbWidgetSource",
                column: "ReleaseWidgetId",
                principalTable: "RbWidgetSource",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetSource_RbWidgetSource_ReleaseWidgetId",
                table: "RbWidgetSource");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetSource_ReleaseWidgetId",
                table: "RbWidgetSource");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "RbWidgetSource");

            migrationBuilder.DropColumn(
                name: "ReleaseWidgetId",
                table: "RbWidgetSource");
        }
    }
}
