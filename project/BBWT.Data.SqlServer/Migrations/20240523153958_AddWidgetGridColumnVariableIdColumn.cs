using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddWidgetGridColumnVariableIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariableId",
                table: "RbWidgetGridColumn",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRowSelectable",
                table: "RbWidgetGrid",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetGridColumn_VariableId",
                table: "RbWidgetGridColumn",
                column: "VariableId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetGridColumn_RbVariable_VariableId",
                table: "RbWidgetGridColumn",
                column: "VariableId",
                principalTable: "RbVariable",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetGridColumn_RbVariable_VariableId",
                table: "RbWidgetGridColumn");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetGridColumn_VariableId",
                table: "RbWidgetGridColumn");

            migrationBuilder.DropColumn(
                name: "VariableId",
                table: "RbWidgetGridColumn");

            migrationBuilder.DropColumn(
                name: "IsRowSelectable",
                table: "RbWidgetGrid");
        }
    }
}
