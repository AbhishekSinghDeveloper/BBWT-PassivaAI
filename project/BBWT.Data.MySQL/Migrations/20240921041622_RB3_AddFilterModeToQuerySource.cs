using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddFilterModeToQuerySource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilterMode",
                table: "RbQuerySource",
                type: "int",
                nullable: true,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilterMode",
                table: "RbQuerySource");
        }
    }
}
