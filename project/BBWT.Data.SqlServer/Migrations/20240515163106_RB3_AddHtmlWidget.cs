using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddHtmlWidget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RbWidgetHtml",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InnerHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WidgetSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuerySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetHtml", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetHtml_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RbWidgetHtml_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetHtml_QuerySourceId",
                table: "RbWidgetHtml",
                column: "QuerySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetHtml_WidgetSourceId",
                table: "RbWidgetHtml",
                column: "WidgetSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbWidgetHtml");
        }
    }
}
