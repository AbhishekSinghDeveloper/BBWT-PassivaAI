using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class AddedDatabaseSourceContextId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.DropIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.Sql($@"DELETE FROM DbDocDatabaseSource WHERE Id <> '00000000-0000-0000-0000-000000000000'");
            migrationBuilder.Sql($@"DELETE FROM DbDocFolders WHERE Id <> '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "DbDocDatabaseSource");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DbDocDatabaseSource",
                newName: "ContextId");

            migrationBuilder.AlterColumn<int>(
                name: "DatabaseType",
                table: "DbDocDatabaseSource",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                principalTable: "DbDocDatabaseSource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.DropIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.RenameColumn(
                name: "ContextId",
                table: "DbDocDatabaseSource",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "DatabaseType",
                table: "DbDocDatabaseSource",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "DbDocDatabaseSource",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                principalTable: "DbDocDatabaseSource",
                principalColumn: "Id");
        }
    }
}
