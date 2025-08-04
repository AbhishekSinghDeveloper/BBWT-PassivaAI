using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FormioMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MajorVersion",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "MinorVersion",
                table: "FormDefinition");

            migrationBuilder.RenameColumn(
                name: "VId",
                table: "FormRevision",
                newName: "MinorVersion");

            migrationBuilder.AddColumn<bool>(
                name: "MUFCapable",
                table: "FormRevision",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MajorVersion",
                table: "FormRevision",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ByRequestOnly",
                table: "FormDefinition",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormCategoryId",
                table: "FormDefinition",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurveyId",
                table: "FormData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormDataId = table.Column<int>(type: "int", nullable: true),
                    FormRevisionId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    RequestDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRequest_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormRequest_FormData_FormDataId",
                        column: x => x.FormDataId,
                        principalTable: "FormData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FormRequest_FormRevision_FormRevisionId",
                        column: x => x.FormRevisionId,
                        principalTable: "FormRevision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    FormRevisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiUserFormDefinition_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MultiUserFormDefinition_FormRevision_FormRevisionId",
                        column: x => x.FormRevisionId,
                        principalTable: "FormRevision",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FormRevisionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Survey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Survey_FormRevision_FormRevisionId",
                        column: x => x.FormRevisionId,
                        principalTable: "FormRevision",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormAssociations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormDataId = table.Column<int>(type: "int", nullable: false),
                    MultiUserFormDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ActiveStepSequenceIndex = table.Column<int>(type: "int", nullable: false),
                    TotalSequenceSteps = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormAssociations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiUserFormAssociations_FormData_FormDataId",
                        column: x => x.FormDataId,
                        principalTable: "FormData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiUserFormAssociations_MultiUserFormDefinition_MultiUserFormDefinitionId",
                        column: x => x.MultiUserFormDefinitionId,
                        principalTable: "MultiUserFormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormDefinitionOrganization",
                columns: table => new
                {
                    OrganizationID = table.Column<int>(type: "int", nullable: false),
                    MultiUserFormDefinitionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormDefinitionOrganization", x => new { x.MultiUserFormDefinitionID, x.OrganizationID });
                    table.ForeignKey(
                        name: "FK_MultiUserFormDefinitionOrganization_MultiUserFormDefinition_MultiUserFormDefinitionID",
                        column: x => x.MultiUserFormDefinitionID,
                        principalTable: "MultiUserFormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiUserFormDefinitionOrganization_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormStage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TabComponentKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerTabKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewerStage = table.Column<bool>(type: "bit", nullable: false),
                    StageTargetType = table.Column<int>(type: "int", nullable: false),
                    SequenceStepIndex = table.Column<int>(type: "int", nullable: false),
                    MultiUserFormDefinitionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiUserFormStage_MultiUserFormDefinition_MultiUserFormDefinitionId",
                        column: x => x.MultiUserFormDefinitionId,
                        principalTable: "MultiUserFormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMultiUserFormStage",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "int", nullable: false),
                    MultiUserFormStageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMultiUserFormStage", x => new { x.GroupsId, x.MultiUserFormStageId });
                    table.ForeignKey(
                        name: "FK_GroupMultiUserFormStage_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMultiUserFormStage_MultiUserFormStage_MultiUserFormStageId",
                        column: x => x.MultiUserFormStageId,
                        principalTable: "MultiUserFormStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormAssociationLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MultiUserFormStageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    ExternalUserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFilled = table.Column<bool>(type: "bit", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecurityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MultiUserFormAssociationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormAssociationLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiUserFormAssociationLinks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MultiUserFormAssociationLinks_MultiUserFormAssociations_MultiUserFormAssociationsId",
                        column: x => x.MultiUserFormAssociationsId,
                        principalTable: "MultiUserFormAssociations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MultiUserFormAssociationLinks_MultiUserFormStage_MultiUserFormStageId",
                        column: x => x.MultiUserFormStageId,
                        principalTable: "MultiUserFormStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiUserFormStagePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TabKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    MultiUserFormStageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiUserFormStagePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiUserFormStagePermissions_MultiUserFormStage_MultiUserFormStageId",
                        column: x => x.MultiUserFormStageId,
                        principalTable: "MultiUserFormStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinition_FormCategoryId",
                table: "FormDefinition",
                column: "FormCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FormData_SurveyId",
                table: "FormData",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRequest_FormDataId",
                table: "FormRequest",
                column: "FormDataId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRequest_FormRevisionId",
                table: "FormRequest",
                column: "FormRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRequest_RequesterId",
                table: "FormRequest",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMultiUserFormStage_MultiUserFormStageId",
                table: "GroupMultiUserFormStage",
                column: "MultiUserFormStageId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormAssociationLinks_MultiUserFormAssociationsId",
                table: "MultiUserFormAssociationLinks",
                column: "MultiUserFormAssociationsId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormAssociationLinks_MultiUserFormStageId",
                table: "MultiUserFormAssociationLinks",
                column: "MultiUserFormStageId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormAssociationLinks_UserId",
                table: "MultiUserFormAssociationLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormAssociations_FormDataId",
                table: "MultiUserFormAssociations",
                column: "FormDataId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormAssociations_MultiUserFormDefinitionId",
                table: "MultiUserFormAssociations",
                column: "MultiUserFormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormDefinition_CreatorId",
                table: "MultiUserFormDefinition",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormDefinition_FormRevisionId",
                table: "MultiUserFormDefinition",
                column: "FormRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormDefinitionOrganization_OrganizationID",
                table: "MultiUserFormDefinitionOrganization",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormStage_MultiUserFormDefinitionId",
                table: "MultiUserFormStage",
                column: "MultiUserFormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiUserFormStagePermissions_MultiUserFormStageId",
                table: "MultiUserFormStagePermissions",
                column: "MultiUserFormStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_FormRevisionId",
                table: "Survey",
                column: "FormRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Survey_SurveyId",
                table: "FormData",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinition_FormCategory_FormCategoryId",
                table: "FormDefinition",
                column: "FormCategoryId",
                principalTable: "FormCategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Survey_SurveyId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinition_FormCategory_FormCategoryId",
                table: "FormDefinition");

            migrationBuilder.DropTable(
                name: "FormCategory");

            migrationBuilder.DropTable(
                name: "FormRequest");

            migrationBuilder.DropTable(
                name: "GroupMultiUserFormStage");

            migrationBuilder.DropTable(
                name: "MultiUserFormAssociationLinks");

            migrationBuilder.DropTable(
                name: "MultiUserFormDefinitionOrganization");

            migrationBuilder.DropTable(
                name: "MultiUserFormStagePermissions");

            migrationBuilder.DropTable(
                name: "Survey");

            migrationBuilder.DropTable(
                name: "MultiUserFormAssociations");

            migrationBuilder.DropTable(
                name: "MultiUserFormStage");

            migrationBuilder.DropTable(
                name: "MultiUserFormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormDefinition_FormCategoryId",
                table: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormData_SurveyId",
                table: "FormData");

            migrationBuilder.DropColumn(
                name: "MUFCapable",
                table: "FormRevision");

            migrationBuilder.DropColumn(
                name: "MajorVersion",
                table: "FormRevision");

            migrationBuilder.DropColumn(
                name: "ByRequestOnly",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "FormCategoryId",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "FormData");

            migrationBuilder.RenameColumn(
                name: "MinorVersion",
                table: "FormRevision",
                newName: "VId");

            migrationBuilder.AddColumn<int>(
                name: "MajorVersion",
                table: "FormDefinition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinorVersion",
                table: "FormDefinition",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
