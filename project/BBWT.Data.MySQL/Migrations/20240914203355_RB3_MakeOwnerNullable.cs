using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_MakeOwnerNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard");

            migrationBuilder.DropForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource");

            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetSource_AspNetUsers_OwnerId",
                table: "RbWidgetSource");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbWidgetSource",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbQuerySource",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbDashboard",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

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
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard");

            migrationBuilder.DropForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource");

            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetSource_AspNetUsers_OwnerId",
                table: "RbWidgetSource");

            migrationBuilder.UpdateData(
                table: "RbWidgetSource",
                keyColumn: "OwnerId",
                keyValue: null,
                column: "OwnerId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbWidgetSource",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbQuerySource",
                keyColumn: "OwnerId",
                keyValue: null,
                column: "OwnerId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbQuerySource",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbDashboard",
                keyColumn: "OwnerId",
                keyValue: null,
                column: "OwnerId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "RbDashboard",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RbDashboard_AspNetUsers_OwnerId",
                table: "RbDashboard",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RbQuerySource_AspNetUsers_OwnerId",
                table: "RbQuerySource",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetSource_AspNetUsers_OwnerId",
                table: "RbWidgetSource",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
