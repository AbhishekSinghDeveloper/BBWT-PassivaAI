using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_QueryAlias_Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AxisXQueryAlias",
                table: "RbWidgetChartDataset",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AxisYQueryAlias",
                table: "RbWidgetChartDataset",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QueryAlias",
                table: "RbBuilderQueryColumn",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AxisXQueryAlias",
                table: "RbWidgetChartDataset");

            migrationBuilder.DropColumn(
                name: "AxisYQueryAlias",
                table: "RbWidgetChartDataset");

            migrationBuilder.DropColumn(
                name: "QueryAlias",
                table: "RbBuilderQueryColumn");
        }
    }
}
