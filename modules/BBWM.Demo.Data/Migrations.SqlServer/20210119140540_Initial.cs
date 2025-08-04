using Microsoft.EntityFrameworkCore.Migrations;

namespace BBWM.Demo.Data.Migrations.SqlServer;

public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Colors",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Colors", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataControls",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                InputBox1 = table.Column<string>(nullable: true),
                InputBox2 = table.Column<string>(nullable: true),
                InputBox3 = table.Column<string>(nullable: true),
                InputBox4 = table.Column<string>(nullable: true),
                InputBox5 = table.Column<string>(nullable: true),
                InputBox6 = table.Column<string>(nullable: true),
                InputBox7 = table.Column<string>(nullable: true),
                InputBox8 = table.Column<string>(nullable: true),
                InputBox9 = table.Column<string>(nullable: true),
                InputBox10 = table.Column<string>(nullable: true),
                CheckBox1 = table.Column<bool>(nullable: false),
                CheckBox2 = table.Column<bool>(nullable: false),
                CheckBox3 = table.Column<bool>(nullable: false),
                CheckBox4 = table.Column<bool>(nullable: false),
                CheckBox5 = table.Column<bool>(nullable: false),
                Selected1 = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataControls", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataHomeAddresses",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AddressLine1 = table.Column<string>(nullable: true),
                AddressLine2 = table.Column<string>(nullable: true),
                Town = table.Column<string>(nullable: true),
                PostCode = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataHomeAddresses", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataUserDetails",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(nullable: true),
                Email = table.Column<string>(nullable: true),
                Phone = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexDataUserDetails", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexOfficeWork",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AddressLine1 = table.Column<string>(nullable: true),
                AddressLine2 = table.Column<string>(nullable: true),
                Town = table.Column<string>(nullable: true),
                PostCode = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexOfficeWork", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ComplexRemoteWork",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IsUK = table.Column<bool>(nullable: false),
                PersonalSkype = table.Column<string>(nullable: true),
                CompanySkype = table.Column<string>(nullable: true),
                PersonalEmail = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplexRemoteWork", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(nullable: true),
                CompanyName = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(nullable: true),
                Age = table.Column<int>(nullable: false),
                Phone = table.Column<string>(nullable: true),
                Email = table.Column<string>(nullable: true),
                RegistrationDate = table.Column<DateTime>(type: "date", nullable: false),
                JobRole = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Files",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Label = table.Column<string>(nullable: true),
                Data = table.Column<string>(nullable: true),
                Type = table.Column<int>(nullable: false),
                Expanded = table.Column<bool>(nullable: false),
                ParentId = table.Column<int>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.Id);
                table.ForeignKey(
                    name: "FK_Files_Files_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Files",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pets",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ReportUsers",
            columns: table => new
            {
                UserId = table.Column<string>(nullable: false),
                ReportId = table.Column<Guid>(nullable: false),
                Id = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReportUsers", x => new { x.ReportId, x.UserId });
            });

        migrationBuilder.CreateTable(
            name: "TasksIssues",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TasksIssues", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Cars",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Brand = table.Column<string>(nullable: false),
                Power = table.Column<int>(nullable: false),
                ColorId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cars", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cars_Colors_ColorId",
                    column: x => x.ColorId,
                    principalTable: "Colors",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerCode = table.Column<string>(nullable: true),
                OrderDate = table.Column<DateTime>(type: "date", nullable: true),
                RequiredDate = table.Column<DateTime>(type: "date", nullable: true),
                ShippedDate = table.Column<DateTime>(type: "date", nullable: true),
                IsPaid = table.Column<bool>(nullable: false),
                CustomerId = table.Column<int>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Persons",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(nullable: true),
                Title = table.Column<string>(nullable: true),
                Gender = table.Column<int>(nullable: false),
                Email = table.Column<string>(nullable: true),
                Address = table.Column<string>(nullable: true),
                OrganizationId = table.Column<int>(nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "date", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Persons", x => x.Id);
                table.ForeignKey(
                    name: "FK_Persons_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ComplexDataMoreDetails",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                IsManagement = table.Column<bool>(nullable: false),
                IsCustomer = table.Column<bool>(nullable: false),
                IsDemo = table.Column<bool>(nullable: false),
                SelectedWorkTime = table.Column<int>(nullable: false),
                SelectedTaskId = table.Column<int>(nullable: false)
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
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShowWorkingData = table.Column<bool>(nullable: false),
                SelectedIssueId = table.Column<int>(nullable: false),
                ReportCode = table.Column<string>(nullable: true),
                ReportDescription = table.Column<string>(nullable: true),
                WorkName = table.Column<string>(nullable: true),
                WorkDescription = table.Column<string>(nullable: true),
                SelectedWorkType = table.Column<int>(nullable: false),
                RemoteWorkId = table.Column<int>(nullable: true),
                OfficeWorkId = table.Column<int>(nullable: true)
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
            name: "OrderDetails",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Quantity = table.Column<decimal>(nullable: false),
                Price = table.Column<decimal>(nullable: false),
                IsReseller = table.Column<bool>(nullable: false),
                ProductId = table.Column<int>(nullable: false),
                OrderId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderDetails_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderDetails_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PersonsPets",
            columns: table => new
            {
                PersonId = table.Column<int>(nullable: false),
                PetId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonsPets", x => new { x.PersonId, x.PetId });
                table.ForeignKey(
                    name: "FK_PersonsPets_Persons_PersonId",
                    column: x => x.PersonId,
                    principalTable: "Persons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PersonsPets_Pets_PetId",
                    column: x => x.PetId,
                    principalTable: "Pets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ComplexData",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserDetailsId = table.Column<int>(nullable: true),
                HomeAddressId = table.Column<int>(nullable: true),
                MoreDetailsId = table.Column<int>(nullable: true),
                WorkDetailsId = table.Column<int>(nullable: true),
                ControlsId = table.Column<int>(nullable: true)
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
            name: "IX_Cars_ColorId",
            table: "Cars",
            column: "ColorId");

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

        migrationBuilder.CreateIndex(
            name: "IX_Files_ParentId",
            table: "Files",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetails_OrderId",
            table: "OrderDetails",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetails_ProductId",
            table: "OrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerCode",
            table: "Orders",
            column: "CustomerCode");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_CustomerId",
            table: "Orders",
            column: "CustomerId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_OrderDate",
            table: "Orders",
            column: "OrderDate");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_RequiredDate",
            table: "Orders",
            column: "RequiredDate");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_ShippedDate",
            table: "Orders",
            column: "ShippedDate");

        migrationBuilder.CreateIndex(
            name: "IX_Persons_OrganizationId",
            table: "Persons",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_PersonsPets_PetId",
            table: "PersonsPets",
            column: "PetId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cars");

        migrationBuilder.DropTable(
            name: "ComplexData");

        migrationBuilder.DropTable(
            name: "Employees");

        migrationBuilder.DropTable(
            name: "Files");

        migrationBuilder.DropTable(
            name: "OrderDetails");

        migrationBuilder.DropTable(
            name: "PersonsPets");

        migrationBuilder.DropTable(
            name: "ReportUsers");

        migrationBuilder.DropTable(
            name: "Colors");

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
            name: "Orders");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Persons");

        migrationBuilder.DropTable(
            name: "Pets");

        migrationBuilder.DropTable(
            name: "ComplexOfficeWork");

        migrationBuilder.DropTable(
            name: "ComplexRemoteWork");

        migrationBuilder.DropTable(
            name: "TasksIssues");

        migrationBuilder.DropTable(
            name: "Customers");

        migrationBuilder.DropTable(
            name: "Organizations");
    }
}
