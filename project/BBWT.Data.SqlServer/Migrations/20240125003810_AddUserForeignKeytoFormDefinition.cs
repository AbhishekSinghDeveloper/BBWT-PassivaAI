using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserForeignKeytoFormDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "FormDefinition",
                type: "nvarchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FormDefinition_UserID",
                table: "FormDefinition",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormDefinition_AspNetUsers_UserID",
                table: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_FormDefinition_UserID",
                table: "FormDefinition");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "FormDefinition",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");
        }
    }
}
