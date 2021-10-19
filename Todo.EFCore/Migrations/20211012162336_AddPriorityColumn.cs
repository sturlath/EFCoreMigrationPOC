using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreMigrationWeb.Migrations
{
    public partial class AddPriorityColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Todos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Todos");
        }
    }
}
