using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_removedChartDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbWidgetChartDataset");

            migrationBuilder.AddColumn<string>(
                name: "AxisXQueryAlias",
                table: "RbWidgetChart",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AxisYQueryAlias",
                table: "RbWidgetChart",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetChart",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeriesQueryAlias",
                table: "RbWidgetChart",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChart_QuerySourceId",
                table: "RbWidgetChart",
                column: "QuerySourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetChart_RbQuerySource_QuerySourceId",
                table: "RbWidgetChart",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetChart_RbQuerySource_QuerySourceId",
                table: "RbWidgetChart");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetChart_QuerySourceId",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "AxisXQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "AxisYQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "QuerySourceId",
                table: "RbWidgetChart");

            migrationBuilder.DropColumn(
                name: "SeriesQueryAlias",
                table: "RbWidgetChart");

            migrationBuilder.CreateTable(
                name: "RbWidgetChartDataset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChartId = table.Column<int>(type: "int", nullable: false),
                    QuerySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AxisXQueryAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AxisYQueryAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetChartDataset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RbWidgetChartDataset_RbWidgetChart_ChartId",
                        column: x => x.ChartId,
                        principalTable: "RbWidgetChart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChartDataset_ChartId",
                table: "RbWidgetChartDataset",
                column: "ChartId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChartDataset_QuerySourceId",
                table: "RbWidgetChartDataset",
                column: "QuerySourceId");
        }
    }
}
