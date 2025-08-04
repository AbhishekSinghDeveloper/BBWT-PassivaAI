using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddPublishingAndOwnershipToWidgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "RbWidgetSource",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RbWidgetSourceOrganization",
                columns: table => new
                {
                    OrganizationsId = table.Column<int>(type: "int", nullable: false),
                    WidgetSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetSourceOrganization", x => new { x.OrganizationsId, x.WidgetSourceId });
                    table.ForeignKey(
                        name: "FK_RbWidgetSourceOrganization_Organizations_OrganizationsId",
                        column: x => x.OrganizationsId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbWidgetSourceOrganization_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("UPDATE RbWidgetSource SET OwnerID = (SELECT TOP(1) ID FROM AspNetUsers ORDER BY ID)");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetSource_OwnerId",
                table: "RbWidgetSource",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetSourceOrganization_WidgetSourceId",
                table: "RbWidgetSourceOrganization",
                column: "WidgetSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetSource_AspNetUsers_OwnerId",
                table: "RbWidgetSource",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetSource_AspNetUsers_OwnerId",
                table: "RbWidgetSource");

            migrationBuilder.DropTable(
                name: "RbWidgetSourceOrganization");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetSource_OwnerId",
                table: "RbWidgetSource");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "RbWidgetSource");
        }
    }
}
