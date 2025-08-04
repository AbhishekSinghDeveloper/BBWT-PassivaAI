using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddQuerySourceReleaseQueryIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseQueryId",
                table: "RbQuerySource",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_RbQuerySource_ReleaseQueryId",
                table: "RbQuerySource",
                column: "ReleaseQueryId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbQuerySource_RbQuerySource_ReleaseQueryId",
                table: "RbQuerySource",
                column: "ReleaseQueryId",
                principalTable: "RbQuerySource",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbQuerySource_RbQuerySource_ReleaseQueryId",
                table: "RbQuerySource");

            migrationBuilder.DropIndex(
                name: "IX_RbQuerySource_ReleaseQueryId",
                table: "RbQuerySource");

            migrationBuilder.DropColumn(
                name: "ReleaseQueryId",
                table: "RbQuerySource");
        }
    }
}
