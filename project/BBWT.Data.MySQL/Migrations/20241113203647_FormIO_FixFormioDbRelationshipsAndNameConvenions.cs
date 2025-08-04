using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class FormIO_FixFormioDbRelationshipsAndNameConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_AspNetUsers_UserID",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_FormRevision_FormRevisionId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Organizations_OrganizationID",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_AspNetUsers_UserID",
                table: "FormDataDraft");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_FormRevision_FormRevisionId",
                table: "FormDataDraft");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinitionOrganization_FormDefinition_FormDefinitionID",
                table: "FormDefinitionOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinitionOrganization_Organizations_OrganizationID",
                table: "FormDefinitionOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_FormPrinting_FormDefinition_FormDataID",
                table: "FormPrinting");

            migrationBuilder.RenameColumn(
                name: "OrganizationID",
                table: "MultiUserFormDefinitionOrganization",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "MultiUserFormDefinitionID",
                table: "MultiUserFormDefinitionOrganization",
                newName: "MultiUserFormDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_MultiUserFormDefinitionOrganization_OrganizationID",
                table: "MultiUserFormDefinitionOrganization",
                newName: "IX_MultiUserFormDefinitionOrganization_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "FormPrinting",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FormDataID",
                table: "FormPrinting",
                newName: "FormDataId");

            migrationBuilder.RenameIndex(
                name: "IX_FormPrinting_FormDataID",
                table: "FormPrinting",
                newName: "IX_FormPrinting_FormDataId");

            migrationBuilder.RenameColumn(
                name: "OrganizationID",
                table: "FormDefinitionOrganization",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "FormDefinitionID",
                table: "FormDefinitionOrganization",
                newName: "FormDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormDefinitionOrganization_OrganizationID",
                table: "FormDefinitionOrganization",
                newName: "IX_FormDefinitionOrganization_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "FormDataDraft",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FormRevisionId",
                table: "FormDataDraft",
                newName: "FormDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_UserID",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_FormRevisionId",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_FormDefinitionId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "FormData",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "OrganizationID",
                table: "FormData",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "FormRevisionId",
                table: "FormData",
                newName: "FormDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_UserID",
                table: "FormData",
                newName: "IX_FormData_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_OrganizationID",
                table: "FormData",
                newName: "IX_FormData_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_FormRevisionId",
                table: "FormData",
                newName: "IX_FormData_FormDefinitionId");

            migrationBuilder.Sql("UPDATE FormData SET FormDefinitionId = " +
                                 "(SELECT FormDefinitionId FROM FormRevision WHERE Id = FormData.FormDefinitionId ORDER BY Id LIMIT 1)");
            migrationBuilder.Sql("UPDATE FormDataDraft SET FormDefinitionId = " +
                                 "(SELECT FormDefinitionId FROM FormRevision WHERE Id = FormDataDraft.FormDefinitionId ORDER BY Id LIMIT 1)");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_AspNetUsers_UserId",
                table: "FormData",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_FormDefinition_FormDefinitionId",
                table: "FormData",
                column: "FormDefinitionId",
                principalTable: "FormDefinition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Organizations_OrganizationId",
                table: "FormData",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_AspNetUsers_UserId",
                table: "FormDataDraft",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_FormDefinition_FormDefinitionId",
                table: "FormDataDraft",
                column: "FormDefinitionId",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinitionOrganization_FormDefinition_FormDefinitionId",
                table: "FormDefinitionOrganization",
                column: "FormDefinitionId",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinitionOrganization_Organizations_OrganizationId",
                table: "FormDefinitionOrganization",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormPrinting_FormDefinition_FormDataId",
                table: "FormPrinting",
                column: "FormDataId",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_AspNetUsers_UserId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_FormDefinition_FormDefinitionId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Organizations_OrganizationId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_AspNetUsers_UserId",
                table: "FormDataDraft");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDataDraft_FormDefinition_FormDefinitionId",
                table: "FormDataDraft");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinitionOrganization_FormDefinition_FormDefinitionId",
                table: "FormDefinitionOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinitionOrganization_Organizations_OrganizationId",
                table: "FormDefinitionOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_FormPrinting_FormDefinition_FormDataId",
                table: "FormPrinting");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "MultiUserFormDefinitionOrganization",
                newName: "OrganizationID");

            migrationBuilder.RenameColumn(
                name: "MultiUserFormDefinitionId",
                table: "MultiUserFormDefinitionOrganization",
                newName: "MultiUserFormDefinitionID");

            migrationBuilder.RenameIndex(
                name: "IX_MultiUserFormDefinitionOrganization_OrganizationId",
                table: "MultiUserFormDefinitionOrganization",
                newName: "IX_MultiUserFormDefinitionOrganization_OrganizationID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FormPrinting",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "FormDataId",
                table: "FormPrinting",
                newName: "FormDataID");

            migrationBuilder.RenameIndex(
                name: "IX_FormPrinting_FormDataId",
                table: "FormPrinting",
                newName: "IX_FormPrinting_FormDataID");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "FormDefinitionOrganization",
                newName: "OrganizationID");

            migrationBuilder.RenameColumn(
                name: "FormDefinitionId",
                table: "FormDefinitionOrganization",
                newName: "FormDefinitionID");

            migrationBuilder.RenameIndex(
                name: "IX_FormDefinitionOrganization_OrganizationId",
                table: "FormDefinitionOrganization",
                newName: "IX_FormDefinitionOrganization_OrganizationID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FormDataDraft",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "FormDefinitionId",
                table: "FormDataDraft",
                newName: "FormRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_UserId",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_FormDataDraft_FormDefinitionId",
                table: "FormDataDraft",
                newName: "IX_FormDataDraft_FormRevisionId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FormData",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "FormData",
                newName: "OrganizationID");

            migrationBuilder.RenameColumn(
                name: "FormDefinitionId",
                table: "FormData",
                newName: "FormRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_UserId",
                table: "FormData",
                newName: "IX_FormData_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_OrganizationId",
                table: "FormData",
                newName: "IX_FormData_OrganizationID");

            migrationBuilder.RenameIndex(
                name: "IX_FormData_FormDefinitionId",
                table: "FormData",
                newName: "IX_FormData_FormRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_AspNetUsers_UserID",
                table: "FormData",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_FormRevision_FormRevisionId",
                table: "FormData",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Organizations_OrganizationID",
                table: "FormData",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_AspNetUsers_UserID",
                table: "FormDataDraft",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDataDraft_FormRevision_FormRevisionId",
                table: "FormDataDraft",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinitionOrganization_FormDefinition_FormDefinitionID",
                table: "FormDefinitionOrganization",
                column: "FormDefinitionID",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinitionOrganization_Organizations_OrganizationID",
                table: "FormDefinitionOrganization",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormPrinting_FormDefinition_FormDataID",
                table: "FormPrinting",
                column: "FormDataID",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
