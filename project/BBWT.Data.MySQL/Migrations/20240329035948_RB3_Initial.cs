using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RbDashboard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbDashboard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RbQuerySource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QueryType = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbQuerySource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RbWidgetSource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WidgetType = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RbBuilderQuery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuerySourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQuery_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbDashboardWidget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DashboardId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WidgetSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbDashboardWidget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbDashboardWidget_RbDashboard_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "RbDashboard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbDashboardWidget_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbWidgetChart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WidgetSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetChart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetChart_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbWidgetGrid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WidgetSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    QuerySourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetGrid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetGrid_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbWidgetGrid_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbBuilderQueryTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QueryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQueryTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQueryTable_RbBuilderQuery_QueryId",
                        column: x => x.QueryId,
                        principalTable: "RbBuilderQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbWidgetChartDataset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuerySourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChartId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetChartDataset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetChartDataset_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbWidgetChartDataset_RbWidgetChart_ChartId",
                        column: x => x.ChartId,
                        principalTable: "RbWidgetChart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbBuilderQueryColumn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQueryColumn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQueryColumn_RbBuilderQueryTable_TableId",
                        column: x => x.TableId,
                        principalTable: "RbBuilderQueryTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQuery_QuerySourceId",
                table: "RbBuilderQuery",
                column: "QuerySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQueryColumn_TableId",
                table: "RbBuilderQueryColumn",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQueryTable_QueryId",
                table: "RbBuilderQueryTable",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_RbDashboardWidget_DashboardId",
                table: "RbDashboardWidget",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_RbDashboardWidget_WidgetSourceId",
                table: "RbDashboardWidget",
                column: "WidgetSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChart_WidgetSourceId",
                table: "RbWidgetChart",
                column: "WidgetSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChartDataset_ChartId",
                table: "RbWidgetChartDataset",
                column: "ChartId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetChartDataset_QuerySourceId",
                table: "RbWidgetChartDataset",
                column: "QuerySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetGrid_QuerySourceId",
                table: "RbWidgetGrid",
                column: "QuerySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetGrid_WidgetSourceId",
                table: "RbWidgetGrid",
                column: "WidgetSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbBuilderQueryColumn");

            migrationBuilder.DropTable(
                name: "RbDashboardWidget");

            migrationBuilder.DropTable(
                name: "RbWidgetChartDataset");

            migrationBuilder.DropTable(
                name: "RbWidgetGrid");

            migrationBuilder.DropTable(
                name: "RbBuilderQueryTable");

            migrationBuilder.DropTable(
                name: "RbDashboard");

            migrationBuilder.DropTable(
                name: "RbWidgetChart");

            migrationBuilder.DropTable(
                name: "RbBuilderQuery");

            migrationBuilder.DropTable(
                name: "RbWidgetSource");

            migrationBuilder.DropTable(
                name: "RbQuerySource");
        }
    }
}
