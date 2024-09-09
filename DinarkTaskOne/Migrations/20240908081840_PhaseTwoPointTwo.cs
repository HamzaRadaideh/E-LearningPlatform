using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinarkTaskOne.Migrations
{
    /// <inheritdoc />
    public partial class PhaseTwoPointTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfOptions",
                table: "Question");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfOptions",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
