using Microsoft.EntityFrameworkCore.Migrations;

namespace ModelScoutAPI.Migrations
{
    public partial class FixNameStatusToVkAcc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientStatus",
                table: "VkAccs",
                newName: "VkAccStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VkAccStatus",
                table: "VkAccs",
                newName: "ClientStatus");
        }
    }
}
