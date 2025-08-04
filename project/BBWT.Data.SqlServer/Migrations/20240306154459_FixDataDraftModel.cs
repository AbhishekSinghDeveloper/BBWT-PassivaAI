using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FixDataDraftModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_FormDefinition_FormDefinitionID",
                table: "FormDataDraft");

            migrationBuilder.RenameColumn(
                name: "FormDefinitionID",
                table: "FormDataDraft",
                newName: "FormRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_FormDefinitionID",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_FormRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_FormRevision_FormRevisionId",
                table: "FormDataDraft",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_FormRevision_FormRevisionId",
                table: "FormDataDraft");

            migrationBuilder.RenameColumn(
                name: "FormRevisionId",
                table: "FormDataDraft",
                newName: "FormDefinitionID");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_FormRevisionId",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_FormDefinitionID");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_FormDefinition_FormDefinitionID",
                table: "FormDataDraft",
                column: "FormDefinitionID",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
