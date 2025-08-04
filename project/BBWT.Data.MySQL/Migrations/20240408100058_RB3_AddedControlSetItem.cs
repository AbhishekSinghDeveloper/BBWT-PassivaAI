using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddedControlSetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WidgetType",
                table: "RbWidgetSource",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "QueryType",
                table: "RbQuerySource",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbDashboard",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateTable(
                name: "RbWidgetControlSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WidgetSourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetControlSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetControlSet_RbWidgetSource_WidgetSourceId",
                        column: x => x.WidgetSourceId,
                        principalTable: "RbWidgetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbWidgetControlSetItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ControlSetId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    HintText = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: true),
                    AutoSubmitInput = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserCanChangeOperator = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExtraSettings = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbWidgetControlSetItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbWidgetControlSetItem_RbWidgetControlSet_ControlSetId",
                        column: x => x.ControlSetId,
                        principalTable: "RbWidgetControlSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetControlSet_WidgetSourceId",
                table: "RbWidgetControlSet",
                column: "WidgetSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbWidgetControlSetItem_ControlSetId",
                table: "RbWidgetControlSetItem",
                column: "ControlSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbWidgetControlSetItem");

            migrationBuilder.DropTable(
                name: "RbWidgetControlSet");

            migrationBuilder.AlterColumn<string>(
                name: "WidgetType",
                table: "RbWidgetSource",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "QueryType",
                table: "RbQuerySource",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbDashboard",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);
        }
    }
}
