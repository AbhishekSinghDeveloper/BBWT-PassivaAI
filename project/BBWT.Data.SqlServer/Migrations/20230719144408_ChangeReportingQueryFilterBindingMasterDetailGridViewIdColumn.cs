using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReportingQueryFilterBindingMasterDetailGridViewIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingGridViews_MasterDetailGridViewId",
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
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailSectionId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingSections_MasterDetailSectionId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailSectionId",
                principalTable: "ReportingSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingQueryFilterBindings_ReportingSections_MasterDetailSectionId",
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
                name: "FK_ReportingQueryFilterBindings_ReportingGridViews_MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailGridViewId",
                principalTable: "ReportingGridViews",
                principalColumn: "Id");
        }
    }
}
