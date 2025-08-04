using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_QueryTableOptionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbWidgetSource",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbQuerySource",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "RbBuilderQueryTable",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "RbBuilderQueryTable",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SelfJoinDbDocColumnId",
                table: "RbBuilderQueryTable",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "RbBuilderQueryTable",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTableId",
                table: "RbBuilderQueryTable",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "RbBuilderQueryColumn",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SourceColumnId",
                table: "RbBuilderQueryColumn",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "RbBuilderQueryTable");

            migrationBuilder.DropColumn(
                name: "OnlyForJoin",
                table: "RbBuilderQueryTable");

            migrationBuilder.DropColumn(
                name: "SelfJoinDbDocColumnId",
                table: "RbBuilderQueryTable");

            migrationBuilder.DropColumn(
                name: "SourceCode",
                table: "RbBuilderQueryTable");

            migrationBuilder.DropColumn(
                name: "SourceTableId",
                table: "RbBuilderQueryTable");

            migrationBuilder.DropColumn(
                name: "OnlyForJoin",
                table: "RbBuilderQueryColumn");

            migrationBuilder.DropColumn(
                name: "SourceColumnId",
                table: "RbBuilderQueryColumn");

            migrationBuilder.UpdateData(
                table: "RbWidgetSource",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbWidgetSource",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbQuerySource",
                keyColumn: "Name",
                keyValue: null,
                column: "Name",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbQuerySource",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
