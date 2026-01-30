using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolyo.Migrations
{
    public partial class AddAboutExtraFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AboutMeTable",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AboutMeTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "AboutMeTable",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "AboutMeTable");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AboutMeTable");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "AboutMeTable");
        }
    }
}
