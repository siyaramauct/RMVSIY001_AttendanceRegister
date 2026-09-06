using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RazorPagesAttendanceRegister.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "AttendanceRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Method",
                table: "AttendanceRecords");
        }
    }
}
