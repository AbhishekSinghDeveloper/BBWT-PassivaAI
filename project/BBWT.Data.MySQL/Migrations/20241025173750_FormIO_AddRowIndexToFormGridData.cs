using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class FormIO_AddRowIndexToFormGridData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentRowIndex",
                table: "FormDataGrid",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowIndex",
                table: "FormDataGrid",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentRowIndex",
                table: "FormDataGrid");

            migrationBuilder.DropColumn(
                name: "RowIndex",
                table: "FormDataGrid");
        }
    }
}
