using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.MySql;

public partial class RemoveComplexDataTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ComplexData");

        migrationBuilder.DropTable(
            name: "ComplexDataControls");

        migrationBuilder.DropTable(
            name: "ComplexDataHomeAddresses");

        migrationBuilder.DropTable(
            name: "ComplexDataMoreDetails");

        migrationBuilder.DropTable(
            name: "ComplexDataUserDetails");

        migrationBuilder.DropTable(
            name: "ComplexDataWorkDetails");

        migrationBuilder.DropTable(
            name: "ComplexOfficeWork");

        migrationBuilder.DropTable(
            name: "ComplexRemoteWork");

        migrationBuilder.DropTable(
            name: "TasksIssues");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ComplexDataControls",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CheckBox1 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CheckBox2 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CheckBox3 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CheckBox4 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CheckBox5 = table.Column<bool>(type: "tinyint(1)", nullable: false),
                InputBox1 = table.Column<string>(type: "longtext", nullable: true),
                InputBox10 = table.Column<string>(type: "longtext", nullable: true),
                InputBox2 = table.Column<string>(type: "longtext", nullable: true),
                InputBox3 = table.Column<string>(type: "longtext", nullable: true),
                InputBox4 = table.Column<string>(type: "longtext", nullable: true),
                InputBox5 = table.Column<string>(type: "longtext", nullable: true),
                InputBox6 = table.Column<string>(type: "longtext", nullable: true),
                InputBox7 = table.Column<string>(type: "longtext", nullable: true),
                InputBox8 = table.Column<string>(type: "longtext", nullable: true),
                InputBox9 = table.Column<string>(type: "longtext", nullable: true),
                Selected1 = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataControls", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataHomeAddresses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                AddressLine1 = table.Column<string>(type: "longtext", nullable: true),
                AddressLine2 = table.Column<string>(type: "longtext", nullable: true),
                PostCode = table.Column<string>(type: "longtext", nullable: true),
                Town = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataHomeAddresses", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataUserDetails",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Email = table.Column<string>(type: "longtext", nullable: true),
                Name = table.Column<string>(type: "longtext", nullable: true),
                Phone = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataUserDetails", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexOfficeWork",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                AddressLine1 = table.Column<string>(type: "longtext", nullable: true),
                AddressLine2 = table.Column<string>(type: "longtext", nullable: true),
                PostCode = table.Column<string>(type: "longtext", nullable: true),
                Town = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexOfficeWork", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexRemoteWork",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                CompanySkype = table.Column<string>(type: "longtext", nullable: true),
                IsUK = table.Column<bool>(type: "tinyint(1)", nullable: false),
                PersonalEmail = table.Column<string>(type: "longtext", nullable: true),
                PersonalSkype = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexRemoteWork", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TasksIssues",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TasksIssues", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataMoreDetails",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                IsCustomer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsDemo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsManagement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                SelectedTaskId = table.Column<int>(type: "int", nullable: false),
                SelectedWorkTime = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataMoreDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_ComplexDataMoreDetails_TasksIssues_SelectedTaskId",
                    column: x => x.SelectedTaskId,
                    principalTable: "TasksIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataWorkDetails",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                OfficeWorkId = table.Column<int>(type: "int", nullable: true),
                RemoteWorkId = table.Column<int>(type: "int", nullable: true),
                ReportCode = table.Column<string>(type: "longtext", nullable: true),
                ReportDescription = table.Column<string>(type: "longtext", nullable: true),
                SelectedIssueId = table.Column<int>(type: "int", nullable: false),
                SelectedWorkType = table.Column<int>(type: "int", nullable: false),
                ShowWorkingData = table.Column<bool>(type: "tinyint(1)", nullable: false),
                WorkDescription = table.Column<string>(type: "longtext", nullable: true),
                WorkName = table.Column<string>(type: "longtext", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataWorkDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_ComplexDataWorkDetails_ComplexOfficeWork_OfficeWorkId",
                    column: x => x.OfficeWorkId,
                    principalTable: "ComplexOfficeWork",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexDataWorkDetails_ComplexRemoteWork_RemoteWorkId",
                    column: x => x.RemoteWorkId,
                    principalTable: "ComplexRemoteWork",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexDataWorkDetails_TasksIssues_SelectedIssueId",
                    column: x => x.SelectedIssueId,
                    principalTable: "TasksIssues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ComplexData",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ControlsId = table.Column<int>(type: "int", nullable: true),
                HomeAddressId = table.Column<int>(type: "int", nullable: true),
                MoreDetailsId = table.Column<int>(type: "int", nullable: true),
                UserDetailsId = table.Column<int>(type: "int", nullable: true),
                WorkDetailsId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexData", x => x.Id);
                table.ForeignKey(
                    name: "FK_ComplexData_ComplexDataControls_ControlsId",
                    column: x => x.ControlsId,
                    principalTable: "ComplexDataControls",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexData_ComplexDataHomeAddresses_HomeAddressId",
                    column: x => x.HomeAddressId,
                    principalTable: "ComplexDataHomeAddresses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexData_ComplexDataMoreDetails_MoreDetailsId",
                    column: x => x.MoreDetailsId,
                    principalTable: "ComplexDataMoreDetails",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexData_ComplexDataUserDetails_UserDetailsId",
                    column: x => x.UserDetailsId,
                    principalTable: "ComplexDataUserDetails",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ComplexData_ComplexDataWorkDetails_WorkDetailsId",
                    column: x => x.WorkDetailsId,
                    principalTable: "ComplexDataWorkDetails",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ComplexData_ControlsId",
            table: "ComplexData",
            column: "ControlsId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexData_HomeAddressId",
            table: "ComplexData",
            column: "HomeAddressId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexData_MoreDetailsId",
            table: "ComplexData",
            column: "MoreDetailsId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexData_UserDetailsId",
            table: "ComplexData",
            column: "UserDetailsId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexData_WorkDetailsId",
            table: "ComplexData",
            column: "WorkDetailsId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexDataMoreDetails_SelectedTaskId",
            table: "ComplexDataMoreDetails",
            column: "SelectedTaskId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexDataWorkDetails_OfficeWorkId",
            table: "ComplexDataWorkDetails",
            column: "OfficeWorkId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexDataWorkDetails_RemoteWorkId",
            table: "ComplexDataWorkDetails",
            column: "RemoteWorkId");

        migrationBuilder.CreateIndex(
            name: "IX_ComplexDataWorkDetails_SelectedIssueId",
            table: "ComplexDataWorkDetails",
            column: "SelectedIssueId");
    }
}
