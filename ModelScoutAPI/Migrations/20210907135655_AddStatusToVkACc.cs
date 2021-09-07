using Microsoft.EntityFrameworkCore.Migrations;

namespace ModelScoutAPI.Migrations
{
    public partial class AddStatusToVkACc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientStatus",
                table: "VkAccs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientStatus",
                table: "VkAccs");
        }
    }
}
