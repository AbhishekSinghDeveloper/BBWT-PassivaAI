using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivationTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedIp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddressFirst = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddressLast = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedIp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedFields = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AuthenticatorRequired = table.Column<bool>(type: "bit", nullable: false),
                    CheckIp = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Datetime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fingerprint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbDocColumnValidationMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rules = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocColumnValidationMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbDocColumnViewMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocColumnViewMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbDocFolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owners = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocFolders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplateParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    From = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventBridgeJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JobId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBridgeJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventBridgeJobsHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JobId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionStatus = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBridgeJobsHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventBridgeRunningJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JobId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CancelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBridgeRunningJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FilesDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsImage = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadingTime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<int>(type: "int", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Account = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadingTime", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LockedOutIp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockedOutIp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DbDocFolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForEndUserOnly = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    MySqlCodeTemplate = table.Column<string>(type: "text", nullable: true),
                    MsSqlCodeTemplate = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alias = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentPreview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPasswordFailedHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    failedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasswordFailedHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedIpRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowedIpId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedIpRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedIpRoles_AllowedIp_AllowedIpId",
                        column: x => x.AllowedIpId,
                        principalTable: "AllowedIp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllowedIpRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbDocColumnType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<int>(type: "int", nullable: false),
                    AnonymizationRule = table.Column<int>(type: "int", nullable: true),
                    ViewMetadataId = table.Column<int>(type: "int", nullable: true),
                    ValidationMetadataId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocColumnType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbDocColumnType_DbDocColumnValidationMetadata_ValidationMetadataId",
                        column: x => x.ValidationMetadataId,
                        principalTable: "DbDocColumnValidationMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DbDocColumnType_DbDocColumnViewMetadata_ViewMetadataId",
                        column: x => x.ViewMetadataId,
                        principalTable: "DbDocColumnViewMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DbDocGridColumnViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinWidth = table.Column<float>(type: "real", nullable: true),
                    MaxWidth = table.Column<float>(type: "real", nullable: true),
                    Mask = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColumnViewMetadataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocGridColumnViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbDocGridColumnViews_DbDocColumnViewMetadata_ColumnViewMetadataId",
                        column: x => x.ColumnViewMetadataId,
                        principalTable: "DbDocColumnViewMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbDocTableMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Anonymization = table.Column<int>(type: "int", nullable: true),
                    Representation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocTableMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbDocTableMetadata_DbDocFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "DbDocFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Brandings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Theme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Disabled = table.Column<bool>(type: "bit", nullable: false),
                    LogoImageId = table.Column<int>(type: "int", nullable: true),
                    LogoIconId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brandings_FilesDetails_LogoIconId",
                        column: x => x.LogoIconId,
                        principalTable: "FilesDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Brandings_FilesDetails_LogoImageId",
                        column: x => x.LogoImageId,
                        principalTable: "FilesDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryFilterSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionalOperator = table.Column<int>(type: "int", nullable: false),
                    QueryId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ParentQueryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryFilterSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterSets_ReportingQueries_ParentQueryId",
                        column: x => x.ParentQueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterSets_ReportingQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterSets_ReportingQueryFilterSets_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ReportingQueryFilterSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DbDocTableId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SelfJoinDbDocColumnId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QueryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTables_ReportingQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryRuleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    QueryRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryRuleTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryRuleTypes_ReportingQueryRules_QueryRuleId",
                        column: x => x.QueryRuleId,
                        principalTable: "ReportingQueryRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbDocColumnMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnonymizationRule = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    ViewMetadataId = table.Column<int>(type: "int", nullable: true),
                    ValidationMetadataId = table.Column<int>(type: "int", nullable: true),
                    ColumnTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbDocColumnMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbDocColumnMetadata_DbDocColumnType_ColumnTypeId",
                        column: x => x.ColumnTypeId,
                        principalTable: "DbDocColumnType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DbDocColumnMetadata_DbDocColumnValidationMetadata_ValidationMetadataId",
                        column: x => x.ValidationMetadataId,
                        principalTable: "DbDocColumnValidationMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DbDocColumnMetadata_DbDocColumnViewMetadata_ViewMetadataId",
                        column: x => x.ViewMetadataId,
                        principalTable: "DbDocColumnViewMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DbDocColumnMetadata_DbDocTableMetadata_TableId",
                        column: x => x.TableId,
                        principalTable: "DbDocTableMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    BrandingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companies_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Companies_Brandings_BrandingId",
                        column: x => x.BrandingId,
                        principalTable: "Brandings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryTableColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DbDocColumnId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryTableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryTableColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryTableColumns_ReportingQueryTables_QueryTableId",
                        column: x => x.QueryTableId,
                        principalTable: "ReportingQueryTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    AccountStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 4),
                    PreviousAccountStatus = table.Column<int>(type: "int", nullable: true),
                    SsoProvider = table.Column<int>(type: "int", nullable: true),
                    FirstPasswordFailureDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GravatarImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GravatarEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureMode = table.Column<int>(type: "int", nullable: false),
                    U2fEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RecoveryCode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    AvatarImageId = table.Column<int>(type: "int", nullable: true),
                    InvitationTokenId = table.Column<int>(type: "int", nullable: true),
                    PasswordResetTokenId = table.Column<int>(type: "int", nullable: true),
                    EmailConfirmationTokenId = table.Column<int>(type: "int", nullable: true),
                    LastLoginBrowserFingerprint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthSecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_ActivationTokens_EmailConfirmationTokenId",
                        column: x => x.EmailConfirmationTokenId,
                        principalTable: "ActivationTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_ActivationTokens_InvitationTokenId",
                        column: x => x.InvitationTokenId,
                        principalTable: "ActivationTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_ActivationTokens_PasswordResetTokenId",
                        column: x => x.PasswordResetTokenId,
                        principalTable: "ActivationTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_FilesDetails_AvatarImageId",
                        column: x => x.AvatarImageId,
                        principalTable: "FilesDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomSqlCodeTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Value2 = table.Column<string>(type: "text", nullable: true),
                    QueryFilterSetId = table.Column<int>(type: "int", nullable: false),
                    QueryTableColumnId = table.Column<int>(type: "int", nullable: false),
                    QueryRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilters_ReportingQueryFilterSets_QueryFilterSetId",
                        column: x => x.QueryFilterSetId,
                        principalTable: "ReportingQueryFilterSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilters_ReportingQueryRules_QueryRuleId",
                        column: x => x.QueryRuleId,
                        principalTable: "ReportingQueryRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilters_ReportingQueryTableColumns_QueryTableColumnId",
                        column: x => x.QueryTableColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AllowedIpUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowedIpId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedIpUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedIpUsers_AllowedIp_AllowedIpId",
                        column: x => x.AllowedIpId,
                        principalTable: "AllowedIp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllowedIpUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyHandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Challenge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KeyHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    AttestationCert = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Counter = table.Column<int>(type: "int", nullable: false),
                    IsCompromised = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_AspNetUsers_LockedByUserId",
                        column: x => x.LockedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Metadata_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportingNamedQueries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    QueryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingNamedQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingNamedQueries_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingNamedQueries_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingNamedQueries_ReportingQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UrlSlug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Access = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    PublishedReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingReports_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingReports_AspNetUsers_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingReports_ReportingReports_PublishedReportId",
                        column: x => x.PublishedReportId,
                        principalTable: "ReportingReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserGroups",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserGroups", x => new { x.UserId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_AspNetUserGroups_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingPermissions",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingPermissions", x => new { x.ReportId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_ReportingPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingPermissions_ReportingReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportingReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingRoles",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingRoles", x => new { x.ReportId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ReportingRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingRoles_ReportingReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportingReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ExpandBehaviour = table.Column<int>(type: "int", nullable: false),
                    AutoCollapse = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    PublishedSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NamedQueryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReusedSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QueryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingSections_ReportingNamedQueries_NamedQueryId",
                        column: x => x.NamedQueryId,
                        principalTable: "ReportingNamedQueries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingSections_ReportingQueries_QueryId",
                        column: x => x.QueryId,
                        principalTable: "ReportingQueries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingSections_ReportingReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportingReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingSections_ReportingSections_ReusedSectionId",
                        column: x => x.ReusedSectionId,
                        principalTable: "ReportingSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportingViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingViews_ReportingSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ReportingSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingFilterControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    HintText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    AutoSubmitInput = table.Column<bool>(type: "bit", nullable: false),
                    UserCanChangeOperator = table.Column<bool>(type: "bit", nullable: false),
                    ExtraSettings = table.Column<string>(type: "text", nullable: true),
                    ViewId = table.Column<int>(type: "int", nullable: false),
                    MasterControlId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingFilterControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingFilterControls_ReportingFilterControls_MasterControlId",
                        column: x => x.MasterControlId,
                        principalTable: "ReportingFilterControls",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingFilterControls_ReportingViews_ViewId",
                        column: x => x.ViewId,
                        principalTable: "ReportingViews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingGridViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultSortOrder = table.Column<int>(type: "int", nullable: false),
                    ShowVisibleColumnsSelector = table.Column<bool>(type: "bit", nullable: false),
                    SummaryFooterVisible = table.Column<bool>(type: "bit", nullable: false),
                    ViewId = table.Column<int>(type: "int", nullable: false),
                    DefaultSortColumnId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingGridViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingGridViews_ReportingQueryTableColumns_DefaultSortColumnId",
                        column: x => x.DefaultSortColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingGridViews_ReportingViews_ViewId",
                        column: x => x.ViewId,
                        principalTable: "ReportingViews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingGridViewColumns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    InheritHeader = table.Column<bool>(type: "bit", nullable: false),
                    Header = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sortable = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    ExtraSettings = table.Column<string>(type: "text", nullable: true),
                    GridViewId = table.Column<int>(type: "int", nullable: false),
                    QueryTableColumnId = table.Column<int>(type: "int", nullable: false),
                    CustomColumnTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingGridViewColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingGridViewColumns_DbDocColumnType_CustomColumnTypeId",
                        column: x => x.CustomColumnTypeId,
                        principalTable: "DbDocColumnType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingGridViewColumns_ReportingGridViews_GridViewId",
                        column: x => x.GridViewId,
                        principalTable: "ReportingGridViews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingGridViewColumns_ReportingQueryTableColumns_QueryTableColumnId",
                        column: x => x.QueryTableColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportingQueryFilterBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BindingType = table.Column<int>(type: "int", nullable: false),
                    QueryFilterId = table.Column<int>(type: "int", nullable: false),
                    FilterControlId = table.Column<int>(type: "int", nullable: false),
                    MasterDetailGridViewId = table.Column<int>(type: "int", nullable: true),
                    MasterDetailQueryTableColumnId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingQueryFilterBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterBindings_ReportingFilterControls_FilterControlId",
                        column: x => x.FilterControlId,
                        principalTable: "ReportingFilterControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterBindings_ReportingGridViews_MasterDetailGridViewId",
                        column: x => x.MasterDetailGridViewId,
                        principalTable: "ReportingGridViews",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterBindings_ReportingQueryFilters_QueryFilterId",
                        column: x => x.QueryFilterId,
                        principalTable: "ReportingQueryFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportingQueryFilterBindings_ReportingQueryTableColumns_MasterDetailQueryTableColumnId",
                        column: x => x.MasterDetailQueryTableColumnId,
                        principalTable: "ReportingQueryTableColumns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIpRoles_AllowedIpId",
                table: "AllowedIpRoles",
                column: "AllowedIpId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIpRoles_RoleId",
                table: "AllowedIpRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIpUsers_AllowedIpId",
                table: "AllowedIpUsers",
                column: "AllowedIpId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedIpUsers_UserId",
                table: "AllowedIpUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserGroups_GroupId",
                table: "AspNetUserGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AvatarImageId",
                table: "AspNetUsers",
                column: "AvatarImageId",
                unique: true,
                filter: "[AvatarImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmailConfirmationTokenId",
                table: "AspNetUsers",
                column: "EmailConfirmationTokenId",
                unique: true,
                filter: "[EmailConfirmationTokenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InvitationTokenId",
                table: "AspNetUsers",
                column: "InvitationTokenId",
                unique: true,
                filter: "[InvitationTokenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PasswordResetTokenId",
                table: "AspNetUsers",
                column: "PasswordResetTokenId",
                unique: true,
                filter: "[PasswordResetTokenId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationRequests_UserId",
                table: "AuthenticationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Brandings_LogoIconId",
                table: "Brandings",
                column: "LogoIconId",
                unique: true,
                filter: "[LogoIconId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Brandings_LogoImageId",
                table: "Brandings",
                column: "LogoImageId",
                unique: true,
                filter: "[LogoImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_AddressId",
                table: "Companies",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_BrandingId",
                table: "Companies",
                column: "BrandingId",
                unique: true,
                filter: "[BrandingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnMetadata_ColumnTypeId",
                table: "DbDocColumnMetadata",
                column: "ColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnMetadata_TableId",
                table: "DbDocColumnMetadata",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnMetadata_ValidationMetadataId",
                table: "DbDocColumnMetadata",
                column: "ValidationMetadataId",
                unique: true,
                filter: "[ValidationMetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnMetadata_ViewMetadataId",
                table: "DbDocColumnMetadata",
                column: "ViewMetadataId",
                unique: true,
                filter: "[ViewMetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnType_ValidationMetadataId",
                table: "DbDocColumnType",
                column: "ValidationMetadataId",
                unique: true,
                filter: "[ValidationMetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocColumnType_ViewMetadataId",
                table: "DbDocColumnType",
                column: "ViewMetadataId",
                unique: true,
                filter: "[ViewMetadataId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DbDocGridColumnViews_ColumnViewMetadataId",
                table: "DbDocGridColumnViews",
                column: "ColumnViewMetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DbDocTableMetadata_FolderId",
                table: "DbDocTableMetadata",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobs_JobId",
                table: "EventBridgeJobs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobs_RuleId",
                table: "EventBridgeJobs",
                column: "RuleId",
                unique: true,
                filter: "[RuleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobsHistory_FinishTime",
                table: "EventBridgeJobsHistory",
                column: "FinishTime");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobsHistory_JobId",
                table: "EventBridgeJobsHistory",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobsHistory_RuleId",
                table: "EventBridgeJobsHistory",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeJobsHistory_StartTime",
                table: "EventBridgeJobsHistory",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeRunningJobs_CancelationId",
                table: "EventBridgeRunningJobs",
                column: "CancelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeRunningJobs_JobId",
                table: "EventBridgeRunningJobs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeRunningJobs_RuleId",
                table: "EventBridgeRunningJobs",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EventBridgeRunningJobs_StartTime",
                table: "EventBridgeRunningJobs",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CompanyId",
                table: "Groups",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_LockedByUserId",
                table: "Metadata",
                column: "LockedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_UserId",
                table: "Metadata",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingFilterControls_MasterControlId",
                table: "ReportingFilterControls",
                column: "MasterControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingFilterControls_ViewId",
                table: "ReportingFilterControls",
                column: "ViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViewColumns_CustomColumnTypeId",
                table: "ReportingGridViewColumns",
                column: "CustomColumnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViewColumns_GridViewId",
                table: "ReportingGridViewColumns",
                column: "GridViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViewColumns_QueryTableColumnId",
                table: "ReportingGridViewColumns",
                column: "QueryTableColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViews_DefaultSortColumnId",
                table: "ReportingGridViews",
                column: "DefaultSortColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingGridViews_ViewId",
                table: "ReportingGridViews",
                column: "ViewId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingNamedQueries_CreatedBy",
                table: "ReportingNamedQueries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingNamedQueries_QueryId",
                table: "ReportingNamedQueries",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingNamedQueries_UpdatedBy",
                table: "ReportingNamedQueries",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingPermissions_PermissionId",
                table: "ReportingPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_FilterControlId",
                table: "ReportingQueryFilterBindings",
                column: "FilterControlId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailGridViewId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailGridViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_MasterDetailQueryTableColumnId",
                table: "ReportingQueryFilterBindings",
                column: "MasterDetailQueryTableColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterBindings_QueryFilterId",
                table: "ReportingQueryFilterBindings",
                column: "QueryFilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilters_QueryFilterSetId",
                table: "ReportingQueryFilters",
                column: "QueryFilterSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilters_QueryRuleId",
                table: "ReportingQueryFilters",
                column: "QueryRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilters_QueryTableColumnId",
                table: "ReportingQueryFilters",
                column: "QueryTableColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterSets_ParentId",
                table: "ReportingQueryFilterSets",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterSets_ParentQueryId",
                table: "ReportingQueryFilterSets",
                column: "ParentQueryId",
                unique: true,
                filter: "[ParentQueryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryFilterSets_QueryId",
                table: "ReportingQueryFilterSets",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryRuleTypes_QueryRuleId",
                table: "ReportingQueryRuleTypes",
                column: "QueryRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTableColumns_QueryTableId",
                table: "ReportingQueryTableColumns",
                column: "QueryTableId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingQueryTables_QueryId",
                table: "ReportingQueryTables",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingReports_CreatedBy",
                table: "ReportingReports",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingReports_PublishedReportId",
                table: "ReportingReports",
                column: "PublishedReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingReports_UpdatedBy",
                table: "ReportingReports",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingRoles_RoleId",
                table: "ReportingRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingSections_NamedQueryId",
                table: "ReportingSections",
                column: "NamedQueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingSections_QueryId",
                table: "ReportingSections",
                column: "QueryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingSections_ReportId",
                table: "ReportingSections",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingSections_ReusedSectionId",
                table: "ReportingSections",
                column: "ReusedSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingViews_SectionId",
                table: "ReportingViews",
                column: "SectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedIpRoles");

            migrationBuilder.DropTable(
                name: "AllowedIpUsers");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserGroups");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "AuthenticationRequests");

            migrationBuilder.DropTable(
                name: "DbDocColumnMetadata");

            migrationBuilder.DropTable(
                name: "DbDocGridColumnViews");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "EmailTemplateParameters");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "EventBridgeJobs");

            migrationBuilder.DropTable(
                name: "EventBridgeJobsHistory");

            migrationBuilder.DropTable(
                name: "EventBridgeRunningJobs");

            migrationBuilder.DropTable(
                name: "LoadingTime");

            migrationBuilder.DropTable(
                name: "LockedOutIp");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "PasswordHistory");

            migrationBuilder.DropTable(
                name: "ReportingGridViewColumns");

            migrationBuilder.DropTable(
                name: "ReportingPermissions");

            migrationBuilder.DropTable(
                name: "ReportingQueryFilterBindings");

            migrationBuilder.DropTable(
                name: "ReportingQueryRuleTypes");

            migrationBuilder.DropTable(
                name: "ReportingRoles");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StaticPages");

            migrationBuilder.DropTable(
                name: "UserPasswordFailedHistory");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "AllowedIp");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "DbDocTableMetadata");

            migrationBuilder.DropTable(
                name: "DbDocColumnType");

            migrationBuilder.DropTable(
                name: "ReportingFilterControls");

            migrationBuilder.DropTable(
                name: "ReportingGridViews");

            migrationBuilder.DropTable(
                name: "ReportingQueryFilters");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "DbDocFolders");

            migrationBuilder.DropTable(
                name: "DbDocColumnValidationMetadata");

            migrationBuilder.DropTable(
                name: "DbDocColumnViewMetadata");

            migrationBuilder.DropTable(
                name: "ReportingViews");

            migrationBuilder.DropTable(
                name: "ReportingQueryFilterSets");

            migrationBuilder.DropTable(
                name: "ReportingQueryRules");

            migrationBuilder.DropTable(
                name: "ReportingQueryTableColumns");

            migrationBuilder.DropTable(
                name: "ReportingSections");

            migrationBuilder.DropTable(
                name: "ReportingQueryTables");

            migrationBuilder.DropTable(
                name: "ReportingNamedQueries");

            migrationBuilder.DropTable(
                name: "ReportingReports");

            migrationBuilder.DropTable(
                name: "ReportingQueries");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ActivationTokens");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Brandings");

            migrationBuilder.DropTable(
                name: "FilesDetails");
        }
    }
}
