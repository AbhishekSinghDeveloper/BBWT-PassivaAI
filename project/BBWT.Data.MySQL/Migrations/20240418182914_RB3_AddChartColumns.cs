using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddChartColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AxisXQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "AxisYQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "BubbleSizeQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "SeriesQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.CreateTable(
                name: "RbWidgetChartColumn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QueryAlias = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    ChartAlias = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ColumnPurpose = table.Column<int>(type: "int", nullable: false),
                    ChartId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetChartColumn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetChartColumn_RbWidgetChart_ChartId",
                        column: x => x.ChartId,
                        principalTable: "RbWidgetChart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChartColumn_ChartId",
                table: "RbWidgetChartColumn",
                column: "ChartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbWidgetChartColumn");

            migrationBuilder.AddColumn<string>(
                name: "AxisXQueryAlias",
                table: "RbWidgetChart",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AxisYQueryAlias",
                table: "RbWidgetChart",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BubbleSizeQueryAlias",
                table: "RbWidgetChart",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriesQueryAlias",
                table: "RbWidgetChart",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
