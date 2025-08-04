using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_RemovedAutoBuilderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RbBuilderQueryColumn");

            migrationBuilder.DropTable(
                name: "RbBuilderQueryTable");

            migrationBuilder.DropTable(
                name: "RbBuilderQuery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RbBuilderQuery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuerySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableSetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQuery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQuery_RbQuerySource_QuerySourceId",
                        column: x => x.QuerySourceId,
                        principalTable: "RbQuerySource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RbBuilderQuery_RbTableSet_TableSetId",
                        column: x => x.TableSetId,
                        principalTable: "RbTableSet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RbBuilderQueryTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OnlyForJoin = table.Column<bool>(type: "bit", nullable: false),
                    SelfJoinDbDocColumnId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceTableId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQueryTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQueryTable_RbBuilderQuery_QueryId",
                        column: x => x.QueryId,
                        principalTable: "RbBuilderQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RbBuilderQueryColumn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    OnlyForJoin = table.Column<bool>(type: "bit", nullable: false),
                    QueryAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceColumnId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RbBuilderQueryColumn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RbBuilderQueryColumn_RbBuilderQueryTable_TableId",
                        column: x => x.TableId,
                        principalTable: "RbBuilderQueryTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQuery_QuerySourceId",
                table: "RbBuilderQuery",
                column: "QuerySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQuery_TableSetId",
                table: "RbBuilderQuery",
                column: "TableSetId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQueryColumn_TableId",
                table: "RbBuilderQueryColumn",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_RbBuilderQueryTable_QueryId",
                table: "RbBuilderQueryTable",
                column: "QueryId");
        }
    }
}
