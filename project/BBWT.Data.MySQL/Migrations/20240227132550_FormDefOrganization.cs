using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class FormDefOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationID",
                table: "FormData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormDefinitionOrganization",
                columns: table => new
                {
                    OrganizationID = table.Column<int>(type: "int", nullable: false),
                    FormDefinitionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDefinitionOrganization", x => new { x.FormDefinitionID, x.OrganizationID });
                    table.ForeignKey(
                        name: "FK_FormDefinitionOrganization_FormDefinition_FormDefinitionID",
                        column: x => x.FormDefinitionID,
                        principalTable: "FormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormDefinitionOrganization_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormData_OrganizationID",
                table: "FormData",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinitionOrganization_OrganizationID",
                table: "FormDefinitionOrganization",
                column: "OrganizationID");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_Organizations_OrganizationID",
                table: "FormData",
                column: "OrganizationID",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_Organizations_OrganizationID",
                table: "FormData");

            migrationBuilder.DropTable(
                name: "FormDefinitionOrganization");

            migrationBuilder.DropIndex(
                name: "IX_FormData_OrganizationID",
                table: "FormData");

            migrationBuilder.DropColumn(
                name: "OrganizationID",
                table: "FormData");
        }
    }
}
