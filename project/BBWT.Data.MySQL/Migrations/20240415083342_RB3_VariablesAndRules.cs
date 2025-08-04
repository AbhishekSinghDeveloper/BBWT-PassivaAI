using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_VariablesAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayRuleId",
                table: "RbWidgetSource",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RbVariable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbVariable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RbVariableRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VariableName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    Operand = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbVariableRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetSource_DisplayRuleId",
                table: "RbWidgetSource",
                column: "DisplayRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbWidgetSource_RbVariableRule_DisplayRuleId",
                table: "RbWidgetSource",
                column: "DisplayRuleId",
                principalTable: "RbVariableRule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbWidgetSource_RbVariableRule_DisplayRuleId",
                table: "RbWidgetSource");

            migrationBuilder.DropTable(
                name: "RbVariable");

            migrationBuilder.DropTable(
                name: "RbVariableRule");

            migrationBuilder.DropIndex(
                name: "IX_RbWidgetSource_DisplayRuleId",
                table: "RbWidgetSource");

            migrationBuilder.DropColumn(
                name: "DisplayRuleId",
                table: "RbWidgetSource");
        }
    }
}
