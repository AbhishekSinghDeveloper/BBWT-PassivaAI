using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_RemoveAutoSubmitFieldFromCSItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSubmitInput",
                table: "RbWidgetControlSetItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoSubmitInput",
                table: "RbWidgetControlSetItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
