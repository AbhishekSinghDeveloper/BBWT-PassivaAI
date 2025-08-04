using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.SqlServer;

public partial class RemoveOrderCustomerCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Orders_CustomerCode",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "CustomerCode",
            table: "Orders");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CustomerCode",
            table: "Orders",
            type: "nvarchar(450)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerCode",
            table: "Orders",
            column: "CustomerCode");
    }
}
