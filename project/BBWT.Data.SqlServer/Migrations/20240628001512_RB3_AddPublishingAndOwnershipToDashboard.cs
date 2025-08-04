using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddPublishingAndOwnershipToDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "RbDashboard",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RbDashboardOrganization",
                columns: table => new
                {
                    DashboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbDashboardOrganization", x => new { x.DashboardId, x.OrganizationsId });
                    table.ForeignKey(
                        name: "FK_RbDashboardOrganization_Organizations_OrganizationsId",
                        column: x => x.OrganizationsId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbDashboardOrganization_RbDashboard_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "RbDashboard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("UPDATE RbDashboard SET OwnerID = (SELECT TOP(1) ID FROM AspNetUsers ORDER BY ID)");

            migrationBuilder.CreateIndex(
                name: "IX_RbDashboard_OwnerId",
                table: "RbDashboard",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RbDashboardOrganization_OrganizationsId",
                table: "RbDashboardOrganization",
                column: "OrganizationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard");

            migrationBuilder.DropTable(
                name: "RbDashboardOrganization");

            migrationBuilder.DropIndex(
                name: "IX_RbDashboard_OwnerId",
                table: "RbDashboard");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "RbDashboard");
        }
    }
}
