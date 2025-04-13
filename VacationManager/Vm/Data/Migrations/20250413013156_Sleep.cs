using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vm.Data.Migrations
{
    /// <inheritdoc />
    public partial class Sleep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "VacationRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "VacationRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
