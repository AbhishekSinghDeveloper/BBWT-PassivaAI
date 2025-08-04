using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.MySQL.Migrations
{
    /// <inheritdoc />
    public partial class CreateJobExecutionDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchedulerJobRunDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JobName = table.Column<string>(type: "longtext", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    JobGroup = table.Column<string>(type: "longtext", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    JobType = table.Column<string>(type: "longtext", nullable: true),
                    TriggerType = table.Column<string>(type: "longtext", nullable: true),
                    TriggerGroup = table.Column<string>(type: "longtext", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    ServerName = table.Column<string>(type: "longtext", nullable: true),
                    IsRecurring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    AssemblyName = table.Column<string>(type: "longtext", nullable: true),
                    Cron = table.Column<string>(type: "longtext", nullable: true)

                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJobRunDetails", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerJobRunDetails");
        }
    }
}
