using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class QueryTableToUserTableSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DbDocTableId",
                table: "ReportingQueryTables",
                newName: "SourceTableId");

            migrationBuilder.RenameColumn(
                name: "DbDocColumnId",
                table: "ReportingQueryTableColumns",
                newName: "SourceColumnId");

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "ReportingQueryTables",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCode",
                table: "ReportingQueryTables");

            migrationBuilder.RenameColumn(
                name: "SourceTableId",
                table: "ReportingQueryTables",
                newName: "DbDocTableId");

            migrationBuilder.RenameColumn(
                name: "SourceColumnId",
                table: "ReportingQueryTableColumns",
                newName: "DbDocColumnId");
        }
    }
}
