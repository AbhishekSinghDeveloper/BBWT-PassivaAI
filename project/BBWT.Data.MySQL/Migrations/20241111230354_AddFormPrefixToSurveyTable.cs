using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddFormPrefixToSurveyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Survey_SurveyId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_Survey_FormRevision_FormRevisionId",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_Survey_FormRevisionId",
                table: "Survey");

            migrationBuilder.RenameTable(name: "Survey", newName: "FormSurvey");

            migrationBuilder.CreateIndex(
                name: "IX_FormSurvey_FormRevisionId",
                table: "FormSurvey",
                column: "FormRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormSurvey_FormRevision_FormRevisionId",
                table: "FormSurvey",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_FormSurvey_SurveyId",
                table: "FormData",
                column: "SurveyId",
                principalTable: "FormSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_FormSurvey_SurveyId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormSurvey_FormRevision_FormRevisionId",
                table: "Survey");

            migrationBuilder.DropIndex(
                name: "IX_FormSurvey_FormRevisionId",
                table: "Survey");

            migrationBuilder.RenameTable(name: "FormSurvey", newName: "Survey");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_FormRevisionId",
                table: "FormSurvey",
                column: "FormRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Survey_FormRevision_FormRevisionId",
                table: "FormSurvey",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Survey_SurveyId",
                table: "FormData",
                column: "SurveyId",
                principalTable: "FormSurvey",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
