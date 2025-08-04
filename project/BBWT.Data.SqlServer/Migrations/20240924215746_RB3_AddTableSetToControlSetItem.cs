using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddTableSetToControlSetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TableSetId",
                table: "RbWidgetControlSetItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetControlSetItem_TableSetId",
                table: "RbWidgetControlSetItem",
                column: "TableSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetControlSetItem_RbTableSet_TableSetId",
                table: "RbWidgetControlSetItem",
                column: "TableSetId",
                principalTable: "RbTableSet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetControlSetItem_RbTableSet_TableSetId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetControlSetItem_TableSetId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "TableSetId",
                table: "RbWidgetControlSetItem");
        }
    }
}
