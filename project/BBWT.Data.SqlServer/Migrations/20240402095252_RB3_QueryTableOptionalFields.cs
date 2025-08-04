using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
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
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbQuerySource",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "RbBuilderQueryTable",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "RbBuilderQueryTable",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SelfJoinDbDocColumnId",
                table: "RbBuilderQueryTable",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCode",
                table: "RbBuilderQueryTable",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTableId",
                table: "RbBuilderQueryTable",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "OnlyForJoin",
                table: "RbBuilderQueryColumn",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SourceColumnId",
                table: "RbBuilderQueryColumn",
                type: "nvarchar(500)",
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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbWidgetSource",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RbQuerySource",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
