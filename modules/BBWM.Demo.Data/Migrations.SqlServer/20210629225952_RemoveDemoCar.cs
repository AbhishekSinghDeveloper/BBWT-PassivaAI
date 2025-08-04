using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.SqlServer;

public partial class RemoveDemoCar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cars");

        migrationBuilder.DropTable(
            name: "ReportUsers");

        migrationBuilder.AlterColumn<DateTime>(
            name: "RegistrationDate",
            table: "Employees",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "date");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "RegistrationDate",
            table: "Employees",
            type: "date",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "date",
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "Cars",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Id = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
