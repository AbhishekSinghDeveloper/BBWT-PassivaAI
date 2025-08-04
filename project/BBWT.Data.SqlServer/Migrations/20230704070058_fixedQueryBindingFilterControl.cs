using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class fixedQueryBindingFilterControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingFilterControls_FilterControlId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.AlterColumn<int>(
                name: "FilterControlId",
                table: "ReportingQueryFilterBindings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingFilterControls_FilterControlId",
                table: "ReportingQueryFilterBindings",
                column: "FilterControlId",
                principalTable: "ReportingFilterControls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingFilterControls_FilterControlId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.AlterColumn<int>(
                name: "FilterControlId",
                table: "ReportingQueryFilterBindings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingFilterControls_FilterControlId",
                table: "ReportingQueryFilterBindings",
                column: "FilterControlId",
                principalTable: "ReportingFilterControls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
