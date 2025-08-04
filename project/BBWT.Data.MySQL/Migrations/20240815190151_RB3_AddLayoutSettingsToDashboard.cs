using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddLayoutSettingsToDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Layout",
                table: "RbDashboard",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WidgetsMargin",
                table: "RbDashboard",
                type: "int",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "WidgetsPadding",
                table: "RbDashboard",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.Sql($@"UPDATE RbDashboard SET Layout = 1");
            migrationBuilder.Sql($@"UPDATE RbDashboard SET WidgetsMargin = 20");
            migrationBuilder.Sql($@"UPDATE RbDashboard SET WidgetsPadding = 15");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Layout",
                table: "RbDashboard");

            migrationBuilder.DropColumn(
                name: "WidgetsMargin",
                table: "RbDashboard");

            migrationBuilder.DropColumn(
                name: "WidgetsPadding",
                table: "RbDashboard");
        }
    }
}
