using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddDbDocDatabaseSourceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DatabaseSourceId",
                table: "DbDocFolders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DbDocDatabaseSource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    DatabaseType = table.Column<int>(type: "int", nullable: true),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocDatabaseSource", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId");

            #region custom part
            var guid = Guid.NewGuid();
            migrationBuilder.Sql($@"Insert into DbDocDatabaseSource (Id, Name, SourceType) Values ('{guid}', 'All model contexts', 1)");

            migrationBuilder.Sql($@"UPDATE DbDocFolders SET DbDocFolders.DatabaseSourceId = '{guid}'");

            migrationBuilder.AlterColumn<Guid>(
                name: "DatabaseSourceId",
                table: "DbDocFolders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
            #endregion

            migrationBuilder.AddForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders",
                column: "DatabaseSourceId",
                principalTable: "DbDocDatabaseSource",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbDocFolders_DbDocDatabaseSource_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.DropTable(
                name: "DbDocDatabaseSource");

            migrationBuilder.DropIndex(
                name: "IX_DbDocFolders_DatabaseSourceId",
                table: "DbDocFolders");

            migrationBuilder.DropColumn(
                name: "DatabaseSourceId",
                table: "DbDocFolders");
        }
    }
}
