using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_RemoveQuerySourceFromHTMLWidget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetHtml_RbQuerySource_QuerySourceId",
                table: "RbWidgetHtml");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetHtml_QuerySourceId",
                table: "RbWidgetHtml");

            migrationBuilder.DropColumn(
                name: "QuerySourceId",
                table: "RbWidgetHtml");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetHtml",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetHtml_QuerySourceId",
                table: "RbWidgetHtml",
                column: "QuerySourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetHtml_RbQuerySource_QuerySourceId",
                table: "RbWidgetHtml",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id");
        }
    }
}
