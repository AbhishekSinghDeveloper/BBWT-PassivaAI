using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingQueryTableJoinsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "ReportingQueryTables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "ReportingQueryTableColumns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReportingQueryTableJoins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    FromQueryTableId = table.Column<int>(type: "int", nullable: true),
                    FromQueryTableColumnId = table.Column<int>(type: "int", nullable: true),
                    ToQueryTableId = table.Column<int>(type: "int", nullable: true),
                    ToQueryTableColumnId = table.Column<int>(type: "int", nullable: true),
                    JoinType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryTableJoins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableJoins_ReportingQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableJoins_ReportingQueryTableColumns_FromQueryTableColumnId",
                        column: x => x.FromQueryTableColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableJoins_ReportingQueryTableColumns_ToQueryTableColumnId",
                        column: x => x.ToQueryTableColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableJoins_ReportingQueryTables_FromQueryTableId",
                        column: x => x.FromQueryTableId,
                        principalTable: "ReportingQueryTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableJoins_ReportingQueryTables_ToQueryTableId",
                        column: x => x.ToQueryTableId,
                        principalTable: "ReportingQueryTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableJoins_FromQueryTableColumnId",
                table: "ReportingQueryTableJoins",
                column: "FromQueryTableColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableJoins_FromQueryTableId",
                table: "ReportingQueryTableJoins",
                column: "FromQueryTableId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableJoins_QueryId",
                table: "ReportingQueryTableJoins",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableJoins_ToQueryTableColumnId",
                table: "ReportingQueryTableJoins",
                column: "ToQueryTableColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableJoins_ToQueryTableId",
                table: "ReportingQueryTableJoins",
                column: "ToQueryTableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition");

            migrationBuilder.DropTable(
                name: "ReportingQueryTableJoins");

            migrationBuilder.DropColumn(
                name: "OnlyForJoin",
                table: "ReportingQueryTables");

            migrationBuilder.DropColumn(
                name: "OnlyForJoin",
                table: "ReportingQueryTableColumns");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
