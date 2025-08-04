using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddBubbleChartSizeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BubbleSizeQueryAlias",
                table: "RbWidgetChart",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BubbleSizeQueryAlias",
                table: "RbWidgetChart");
        }
    }
}
