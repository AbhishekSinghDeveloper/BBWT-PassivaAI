using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    public partial class UpdateReportingGridViewDefaultSortColumnColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingGridViews_ReportingQueryTableColumns_DefaultSortCol~",
                table: "ReportingGridViews");

            migrationBuilder.DropIndex(
                name: "IX_ReportingGridViews_DefaultSortColumnId",
                table: "ReportingGridViews");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViews_DefaultSortColumnId",
                table: "ReportingGridViews",
                column: "DefaultSortColumnId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingGridViews_ReportingQueryTableColumns_DefaultSortCol~",
                table: "ReportingGridViews",
                column: "DefaultSortColumnId",
                principalTable: "ReportingQueryTableColumns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportingGridViews_ReportingQueryTableColumns_DefaultSortCol~",
                table: "ReportingGridViews");

            migrationBuilder.DropIndex(
                name: "IX_ReportingGridViews_DefaultSortColumnId",
                table: "ReportingGridViews");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViews_DefaultSortColumnId",
                table: "ReportingGridViews",
                column: "DefaultSortColumnId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportingGridViews_ReportingQueryTableColumns_DefaultSortCol~",
                table: "ReportingGridViews",
                column: "DefaultSortColumnId",
                principalTable: "ReportingQueryTableColumns",
                principalColumn: "Id");
        }
    }
}
