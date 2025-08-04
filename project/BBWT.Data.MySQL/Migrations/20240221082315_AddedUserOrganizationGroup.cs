using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserOrganizationGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganizations_Organizations_OrganizationId",
                table: "UserOrganizations");

            migrationBuilder.CreateTable(
                name: "UserOrganizationGroups",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizationGroups", x => new { x.UserId, x.OrganizationId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_UserOrganizationGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOrganizationGroups_UserOrganizations_UserId_Organization~",
                        columns: x => new { x.UserId, x.OrganizationId },
                        principalTable: "UserOrganizations",
                        principalColumns: new[] { "UserId", "OrganizationId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizationGroups_GroupId",
                table: "UserOrganizationGroups",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOrganizations_Organizations_OrganizationId",
                table: "UserOrganizations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganizations_Organizations_OrganizationId",
                table: "UserOrganizations");

            migrationBuilder.DropTable(
                name: "UserOrganizationGroups");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOrganizations_Organizations_OrganizationId",
                table: "UserOrganizations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
