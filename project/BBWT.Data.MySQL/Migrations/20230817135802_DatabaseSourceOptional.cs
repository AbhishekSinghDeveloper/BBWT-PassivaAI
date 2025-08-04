using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
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
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsSourceFolder",
                table: "DbDocFolders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SchemaCode",
                table: "DbDocDatabaseSource",
                type: "varchar(100)",
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
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

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
