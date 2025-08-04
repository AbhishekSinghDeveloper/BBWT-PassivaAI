using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReportingQueryFilterBindingMasterDetailGridViewIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingGridViews_MasterDetail~",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.DropIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.DropColumn(
                name: "MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.AddColumn<Guid>(
                name: "MasterDetailSectionId",
                table: "ReportingQueryFilterBindings",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailSectionId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingSections_MasterDetailS~",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailSectionId",
                principalTable: "ReportingSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingSections_MasterDetailS~",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.DropIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailSectionId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.DropColumn(
                name: "MasterDetailSectionId",
                table: "ReportingQueryFilterBindings");

            migrationBuilder.AddColumn<int>(
                name: "MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailGridViewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingGridViews_MasterDetail~",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailGridViewId",
                principalTable: "ReportingGridViews",
                principalColumn: "Id");
        }
    }
}
