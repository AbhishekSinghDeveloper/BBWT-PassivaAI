using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddFilterRuleToControlSetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilterRuleId",
                table: "RbWidgetControlSetItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RbFilterRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    TableColumnId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbFilterRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetControlSetItem_FilterRuleId",
                table: "RbWidgetControlSetItem",
                column: "FilterRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetControlSetItem_RbFilterRule_FilterRuleId",
                table: "RbWidgetControlSetItem",
                column: "FilterRuleId",
                principalTable: "RbFilterRule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetControlSetItem_RbFilterRule_FilterRuleId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropTable(
                name: "RbFilterRule");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetControlSetItem_FilterRuleId",
                table: "RbWidgetControlSetItem");

            migrationBuilder.DropColumn(
                name: "FilterRuleId",
                table: "RbWidgetControlSetItem");
        }
    }
}
