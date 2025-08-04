using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class FormIO_RemoveFormDataGrids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormDataGrid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormDataGrid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FormDataId = table.Column<int>(type: "int", nullable: false),
                    FormRevisionGridId = table.Column<int>(type: "int", nullable: true),
                    Json = table.Column<string>(type: "longtext", nullable: false),
                    ParentRowIndex = table.Column<int>(type: "int", nullable: true),
                    RowIndex = table.Column<int>(type: "int", nullable: false)
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
        }
    }
}
