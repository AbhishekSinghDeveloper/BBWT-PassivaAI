using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.MySql;

public partial class removeOrganizationsAndLinkEmployees : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Organizations");

        migrationBuilder.AddColumn<int>(
            name: "EmployeeId",
            table: "Orders",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_EmployeeId",
            table: "Orders",
            column: "EmployeeId");

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Employees_EmployeeId",
            table: "Orders",
            column: "EmployeeId",
            principalTable: "Employees",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Employees_EmployeeId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_EmployeeId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "EmployeeId",
            table: "Orders");

        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
            });
    }
}
