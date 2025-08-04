using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class RB3_AddedTableSetForQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TableSetId",
                table: "RbSqlQuery",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TableSetId",
                table: "RbBuilderQuery",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RbTableSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FolderSourceCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FolderId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbTableSet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbSqlQuery_TableSetId",
                table: "RbSqlQuery",
                column: "TableSetId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQuery_TableSetId",
                table: "RbBuilderQuery",
                column: "TableSetId");

            migrationBuilder.AddForeignKey(
                name: "FK_RbBuilderQuery_RbTableSet_TableSetId",
                table: "RbBuilderQuery",
                column: "TableSetId",
                principalTable: "RbTableSet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RbSqlQuery_RbTableSet_TableSetId",
                table: "RbSqlQuery",
                column: "TableSetId",
                principalTable: "RbTableSet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RbBuilderQuery_RbTableSet_TableSetId",
                table: "RbBuilderQuery");

            migrationBuilder.DropForeignKey(
                name: "FK_RbSqlQuery_RbTableSet_TableSetId",
                table: "RbSqlQuery");

            migrationBuilder.DropTable(
                name: "RbTableSet");

            migrationBuilder.DropIndex(
                name: "IX_RbSqlQuery_TableSetId",
                table: "RbSqlQuery");

            migrationBuilder.DropIndex(
                name: "IX_RbBuilderQuery_TableSetId",
                table: "RbBuilderQuery");

            migrationBuilder.DropColumn(
                name: "TableSetId",
                table: "RbSqlQuery");

            migrationBuilder.DropColumn(
                name: "TableSetId",
                table: "RbBuilderQuery");
        }
    }
}
