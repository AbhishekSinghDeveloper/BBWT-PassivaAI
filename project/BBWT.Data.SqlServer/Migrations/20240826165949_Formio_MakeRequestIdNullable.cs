using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Formio_MakeRequestIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormRequest_AspNetUsers_RequesterId",
                table: "FormRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FormRequest_FormData_FormDataId",
                table: "FormRequest");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterId",
                table: "FormRequest",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)");

            migrationBuilder.AlterColumn<int>(
                name: "FormDataId",
                table: "FormRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_FormRequest_AspNetUsers_RequesterId",
                table: "FormRequest",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormRequest_FormData_FormDataId",
                table: "FormRequest",
                column: "FormDataId",
                principalTable: "FormData",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormRequest_AspNetUsers_RequesterId",
                table: "FormRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_FormRequest_FormData_FormDataId",
                table: "FormRequest");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterId",
                table: "FormRequest",
                type: "nvarchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FormDataId",
                table: "FormRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormRequest_AspNetUsers_RequesterId",
                table: "FormRequest",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormRequest_FormData_FormDataId",
                table: "FormRequest",
                column: "FormDataId",
                principalTable: "FormData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
