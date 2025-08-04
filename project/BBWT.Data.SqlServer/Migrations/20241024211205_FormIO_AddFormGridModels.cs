using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FormIO_AddFormGridModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormRevisionGrid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ParentFormRevisionGridId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRevisionGrid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRevisionGrid_FormDefinition_FormDefinitionId",
                        column: x => x.FormDefinitionId,
                        principalTable: "FormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormRevisionGrid_FormRevisionGrid_ParentFormRevisionGridId",
                        column: x => x.ParentFormRevisionGridId,
                        principalTable: "FormRevisionGrid",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FormDataGrid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormDataId = table.Column<int>(type: "int", nullable: false),
                    FormRevisionGridId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDataGrid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormDataGrid_FormData_FormDataId",
                        column: x => x.FormDataId,
                        principalTable: "FormData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormDataGrid_FormRevisionGrid_FormRevisionGridId",
                        column: x => x.FormRevisionGridId,
                        principalTable: "FormRevisionGrid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormDataGrid_FormDataId",
                table: "FormDataGrid",
                column: "FormDataId");

            migrationBuilder.CreateIndex(
                name: "IX_FormDataGrid_FormRevisionGridId",
                table: "FormDataGrid",
                column: "FormRevisionGridId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevisionGrid_FormDefinitionId",
                table: "FormRevisionGrid",
                column: "FormDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevisionGrid_ParentFormRevisionGridId",
                table: "FormRevisionGrid",
                column: "ParentFormRevisionGridId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormDataGrid");

            migrationBuilder.DropTable(
                name: "FormRevisionGrid");
        }
    }
}
