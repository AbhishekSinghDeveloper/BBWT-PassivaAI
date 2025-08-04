using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.MySql;

public partial class RemoveDemoCar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cars");

        migrationBuilder.DropTable(
            name: "ReportUsers");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Cars",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Brand = table.Column<string>(type: "longtext", nullable: false),
                ColorId = table.Column<int>(type: "int", nullable: false),
                Power = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cars", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cars_Colors_ColorId",
                    column: x => x.ColorId,
                    principalTable: "Colors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ReportUsers",
            columns: table => new
            {
                ReportId = table.Column<Guid>(type: "char(36)", nullable: false),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                Id = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReportUsers", x => new { x.ReportId, x.UserId });
            });

        migrationBuilder.CreateIndex(
            name: "IX_Cars_ColorId",
            table: "Cars",
            column: "ColorId");
    }
}
