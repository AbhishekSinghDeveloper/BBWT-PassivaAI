using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class formio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    MajorVersion = table.Column<int>(type: "int", nullable: false),
                    MinorVersion = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "longtext", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    AvailableIn = table.Column<int>(type: "int", nullable: false),
                    Json = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FormDefinitionID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Json = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormData_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormData_FormDefinition_FormDefinitionID",
                        column: x => x.FormDefinitionID,
                        principalTable: "FormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormPrinting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FormDataID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormPrinting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormPrinting_FormDefinition_FormDataID",
                        column: x => x.FormDataID,
                        principalTable: "FormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormData_FormDefinitionID",
                table: "FormData",
                column: "FormDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_FormData_UserID",
                table: "FormData",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FormPrinting_FormDataID",
                table: "FormPrinting",
                column: "FormDataID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormData");

            migrationBuilder.DropTable(
                name: "FormPrinting");

            migrationBuilder.DropTable(
                name: "FormDefinition");
        }
    }
}
