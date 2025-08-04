using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.MySql;

public partial class ChangeEmployeeRegistrationDateColumnAsOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }
}
