using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    public partial class QueryFilterNotNullFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilters_ReportingQueryRules_QueryRuleId",
                table: "ReportingQueryFilters");

            migrationBuilder.AlterColumn<int>(
                name: "QueryTableColumnId",
                table: "ReportingQueryFilters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QueryRuleId",
                table: "ReportingQueryFilters",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilters_ReportingQueryRules_QueryRuleId",
                table: "ReportingQueryFilters",
                column: "QueryRuleId",
                principalTable: "ReportingQueryRules",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilters_ReportingQueryRules_QueryRuleId",
                table: "ReportingQueryFilters");

            migrationBuilder.AlterColumn<int>(
                name: "QueryTableColumnId",
                table: "ReportingQueryFilters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "QueryRuleId",
                table: "ReportingQueryFilters",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilters_ReportingQueryRules_QueryRuleId",
                table: "ReportingQueryFilters",
                column: "QueryRuleId",
                principalTable: "ReportingQueryRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
