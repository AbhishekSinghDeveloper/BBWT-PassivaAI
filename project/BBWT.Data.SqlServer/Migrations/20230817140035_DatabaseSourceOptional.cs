using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSourceOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.AlterColumn<Guid>(
                name: "DatabaseSourceId",
                table: "DbDocFolders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsSourceFolder",
                table: "DbDocFolders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SchemaCode",
                table: "DbDocDatabaseSource",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                principalTable: "DbDocDatabaseSource",
                principalColumn: "Id");

            migrationBuilder.Sql($@"UPDATE DbDocFolders SET IsSourceFolder = 1 WHERE Name = 'Default Folder' OR Name = 'All Tables'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.DropColumn(
                name: "IsSourceFolder",
                table: "DbDocFolders");

            migrationBuilder.DropColumn(
                name: "SchemaCode",
                table: "DbDocDatabaseSource");

            migrationBuilder.AlterColumn<Guid>(
                name: "DatabaseSourceId",
                table: "DbDocFolders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                principalTable: "DbDocDatabaseSource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
