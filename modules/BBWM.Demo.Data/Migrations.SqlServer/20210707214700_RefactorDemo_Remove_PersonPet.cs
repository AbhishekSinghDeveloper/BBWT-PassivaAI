using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.SqlServer;

public partial class RefactorDemo_Remove_PersonPet : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PersonsPets");

        migrationBuilder.DropTable(
            name: "Persons");

        migrationBuilder.DropTable(
            name: "Pets");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Persons",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Gender = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                OrganizationId = table.Column<int>(type: "int", nullable: true),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Persons", x => x.Id);
                table.ForeignKey(
                    name: "FK_Persons_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Pets",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PersonsPets",
            columns: table => new
            {
                PersonId = table.Column<int>(type: "int", nullable: false),
                PetId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonsPets", x => new { x.PersonId, x.PetId });
                table.ForeignKey(
                    name: "FK_PersonsPets_Persons_PersonId",
                    column: x => x.PersonId,
                    principalTable: "Persons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PersonsPets_Pets_PetId",
                    column: x => x.PetId,
                    principalTable: "Pets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Persons_OrganizationId",
            table: "Persons",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_PersonsPets_PetId",
            table: "PersonsPets",
            column: "PetId");
    }
}
