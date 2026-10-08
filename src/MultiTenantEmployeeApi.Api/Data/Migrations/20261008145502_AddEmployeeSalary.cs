using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantEmployeeApi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "amount_minor",
                table: "employees",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency_code",
                table: "employees",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_minor",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "currency_code",
                table: "employees");
        }
    }
}