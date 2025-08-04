using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingSectionDataViewTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DataViewType",
                table: "ReportingSections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataViewType",
                table: "ReportingSections");
        }
    }
}
