using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddDahsboardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RbDashboard",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DisplayName",
                table: "RbDashboard",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UrlSlug",
                table: "RbDashboard",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RbDashboard");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "RbDashboard");

            migrationBuilder.DropColumn(
                name: "UrlSlug",
                table: "RbDashboard");
        }
    }
}
