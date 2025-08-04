using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_ControlSetVariable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariableId",
                table: "RbWidgetControlSetItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetControlSetItem_VariableId",
                table: "RbWidgetControlSetItem",
                column: "VariableId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetControlSetItem_RbVariable_VariableId",
                table: "RbWidgetControlSetItem",
                column: "VariableId",
                principalTable: "RbVariable",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetControlSetItem_RbVariable_VariableId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetControlSetItem_VariableId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "VariableId",
                table: "RbWidgetControlSetItem");
        }
    }
}
