using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealTime_APIDev.Migrations
{
    /// <inheritdoc />
    public partial class AlterEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeRole",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeRole",
                table: "Employees");
        }
    }
}
