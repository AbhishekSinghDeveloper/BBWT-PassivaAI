using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeWidgetControlSetDropdownManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetControlSetItem_RbTableSet_TableSetId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetControlSetItem_TableSetId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.AddColumn<string>(
                name: "FolderId",
                table: "RbWidgetControlSetItem",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "RbWidgetControlSetItem",
                type: "longtext",
                nullable: true);

            migrationBuilder.Sql("UPDATE RbWidgetControlSetItem SET FolderId = " +
                                 "(SELECT FolderId FROM RbTableSet WHERE Id = RbWidgetControlSetItem.TableSetId ORDER BY Id LIMIT 1) " +
                                 "WHERE True");
            migrationBuilder.Sql("UPDATE RbWidgetControlSetItem SET SourceCode = " +
                                 "(SELECT FolderSourceCode FROM RbTableSet WHERE Id = RbWidgetControlSetItem.TableSetId ORDER BY Id LIMIT 1) " +
                                 "WHERE True");

            migrationBuilder.DropColumn(
                name: "TableSetId",
                table: "RbWidgetControlSetItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "SourceCode",
                table: "RbWidgetControlSetItem");

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
    }
}
