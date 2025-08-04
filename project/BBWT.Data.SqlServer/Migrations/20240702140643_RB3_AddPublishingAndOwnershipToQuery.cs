using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddPublishingAndOwnershipToQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "RbQuerySource",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RbQuerySourceOrganization",
                columns: table => new
                {
                    OrganizationsId = table.Column<int>(type: "int", nullable: false),
                    QuerySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbQuerySourceOrganization", x => new { x.OrganizationsId, x.QuerySourceId });
                    table.ForeignKey(
                        name: "FK_RbQuerySourceOrganization_Organizations_OrganizationsId",
                        column: x => x.OrganizationsId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbQuerySourceOrganization_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("UPDATE RbQuerySource SET OwnerID = (SELECT TOP(1) ID FROM AspNetUsers ORDER BY ID)");

            migrationBuilder.CreateIndex(
                name: "IX_RbQuerySource_OwnerId",
                table: "RbQuerySource",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RbQuerySourceOrganization_QuerySourceId",
                table: "RbQuerySourceOrganization",
                column: "QuerySourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource");

            migrationBuilder.DropTable(
                name: "RbQuerySourceOrganization");

            migrationBuilder.DropIndex(
                name: "IX_RbQuerySource_OwnerId",
                table: "RbQuerySource");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "RbQuerySource");
        }
    }
}
