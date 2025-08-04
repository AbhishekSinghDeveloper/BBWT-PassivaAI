using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBWT.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RB3_RenamedExpressionOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OperatorId",
                table: "RbVariableRule",
                newName: "Operator");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Operator",
                table: "RbVariableRule",
                newName: "OperatorId");
        }
    }
}
