using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_FormRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_FormDefinition_FormDefinitionID",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormDefinition_UserID",
                table: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormData_FormDefinitionID",
                table: "FormData");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "Json",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "MobileFriendly",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "FormDefinitionID",
                table: "FormData");

            migrationBuilder.RenameColumn(
                name: "AvailableIn",
                table: "FormDefinition",
                newName: "ActiveRevisionId");

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                table: "FormDefinition",
                type: "nvarchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormRevisionId",
                table: "FormData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormRevision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MobileFriendly = table.Column<bool>(type: "bit", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormDefinitionId = table.Column<int>(type: "int", nullable: true),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRevision_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormRevision_FormDefinition_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "FormDefinition",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinition_ManagerId",
                table: "FormDefinition",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_FormData_FormRevisionId",
                table: "FormData",
                column: "FormRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevision_CreatorId",
                table: "FormRevision",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevision_FormDefinitionId",
                table: "FormRevision",
                column: "FormDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_FormRevision_FormRevisionId",
                table: "FormData",
                column: "FormRevisionId",
                principalTable: "FormRevision",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinition_AspNetUsers_ManagerId",
                table: "FormDefinition",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormData_FormRevision_FormRevisionId",
                table: "FormData");

            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinition_AspNetUsers_ManagerId",
                table: "FormDefinition");

            migrationBuilder.DropTable(
                name: "FormRevision");

            migrationBuilder.DropIndex(
                name: "IX_FormDefinition_ManagerId",
                table: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormData_FormRevisionId",
                table: "FormData");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "FormDefinition");

            migrationBuilder.DropColumn(
                name: "FormRevisionId",
                table: "FormData");

            migrationBuilder.RenameColumn(
                name: "ActiveRevisionId",
                table: "FormDefinition",
                newName: "AvailableIn");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOn",
                table: "FormDefinition",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "FormDefinition",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MobileFriendly",
                table: "FormDefinition",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "FormDefinition",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FormDefinitionID",
                table: "FormData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinition_UserID",
                table: "FormDefinition",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FormData_FormDefinitionID",
                table: "FormData",
                column: "FormDefinitionID");

            migrationBuilder.AddForeignKey(
                name: "FK_FormData_FormDefinition_FormDefinitionID",
                table: "FormData",
                column: "FormDefinitionID",
                principalTable: "FormDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
