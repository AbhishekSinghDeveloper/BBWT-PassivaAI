using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_WidgetGridColumn_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                table: "RbWidgetChartDataset");

            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetGrid_RbQuerySource_QuerySourceId",
                table: "RbWidgetGrid");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetGrid",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "DefaultSortColumnAlias",
                table: "RbWidgetGrid",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSortOrder",
                table: "RbWidgetGrid",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowVisibleColumnsSelector",
                table: "RbWidgetGrid",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SummaryFooterVisible",
                table: "RbWidgetGrid",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetChartDataset",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "RbWidgetGridColumn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GridId = table.Column<int>(type: "int", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    InheritHeader = table.Column<bool>(type: "bit", nullable: false),
                    Header = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sortable = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    ExtraSettings = table.Column<string>(type: "text", nullable: false),
                    Footer = table.Column<string>(type: "text", nullable: false),
                    CustomColumnTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetGridColumn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetGridColumn_RbWidgetGrid_GridId",
                        column: x => x.GridId,
                        principalTable: "RbWidgetGrid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetGridColumn_GridId",
                table: "RbWidgetGridColumn",
                column: "GridId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                table: "RbWidgetChartDataset",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetGrid_RbQuerySource_QuerySourceId",
                table: "RbWidgetGrid",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                table: "RbWidgetChartDataset");

            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetGrid_RbQuerySource_QuerySourceId",
                table: "RbWidgetGrid");

            migrationBuilder.DropTable(
                name: "RbWidgetGridColumn");

            migrationBuilder.DropColumn(
                name: "DefaultSortColumnAlias",
                table: "RbWidgetGrid");

            migrationBuilder.DropColumn(
                name: "DefaultSortOrder",
                table: "RbWidgetGrid");

            migrationBuilder.DropColumn(
                name: "ShowVisibleColumnsSelector",
                table: "RbWidgetGrid");

            migrationBuilder.DropColumn(
                name: "SummaryFooterVisible",
                table: "RbWidgetGrid");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetGrid",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "QuerySourceId",
                table: "RbWidgetChartDataset",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                table: "RbWidgetChartDataset",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetGrid_RbQuerySource_QuerySourceId",
                table: "RbWidgetGrid",
                column: "QuerySourceId",
                principalTable: "RbQuerySource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
