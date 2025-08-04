using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_QueryAlias_fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "RbWidgetGridColumn");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "RbWidgetGridColumn",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Footer",
                table: "RbWidgetGridColumn",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ExtraSettings",
                table: "RbWidgetGridColumn",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "QueryAlias",
                table: "RbWidgetGridColumn",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AxisYQueryAlias",
                table: "RbWidgetChartDataset",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "AxisXQueryAlias",
                table: "RbWidgetChartDataset",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "RbBuilderQueryTable",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QueryAlias",
                table: "RbBuilderQueryColumn",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueryAlias",
                table: "RbWidgetGridColumn");

            migrationBuilder.UpdateData(
                table: "RbWidgetGridColumn",
                keyColumn: "Header",
                keyValue: null,
                column: "Header",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "RbWidgetGridColumn",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbWidgetGridColumn",
                keyColumn: "Footer",
                keyValue: null,
                column: "Footer",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Footer",
                table: "RbWidgetGridColumn",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbWidgetGridColumn",
                keyColumn: "ExtraSettings",
                keyValue: null,
                column: "ExtraSettings",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ExtraSettings",
                table: "RbWidgetGridColumn",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "RbWidgetGridColumn",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "RbWidgetChartDataset",
                keyColumn: "AxisYQueryAlias",
                keyValue: null,
                column: "AxisYQueryAlias",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "AxisYQueryAlias",
                table: "RbWidgetChartDataset",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RbWidgetChartDataset",
                keyColumn: "AxisXQueryAlias",
                keyValue: null,
                column: "AxisXQueryAlias",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "AxisXQueryAlias",
                table: "RbWidgetChartDataset",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "RbBuilderQueryTable",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QueryAlias",
                table: "RbBuilderQueryColumn",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256);
        }
    }
}
