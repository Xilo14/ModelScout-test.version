using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ModelScoutAPI.Migrations
{
    public partial class _123231 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    LastMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "VkAccs",
                columns: table => new
                {
                    VkAccId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    FriendsLimit = table.Column<int>(type: "integer", nullable: false),
                    CountAddedFriends = table.Column<int>(type: "integer", nullable: false),
                    BirthMonth = table.Column<int>(type: "integer", nullable: false),
                    BirthDay = table.Column<int>(type: "integer", nullable: false),
                    AgeTo = table.Column<int>(type: "integer", nullable: false),
                    AgeFrom = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<int>(type: "integer", nullable: false),
                    Country = table.Column<int>(type: "integer", nullable: false),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VkAccs", x => x.VkAccId);
                    table.ForeignKey(
                        name: "FK_VkAccs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VkClients",
                columns: table => new
                {
                    VkClientId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileVkId = table.Column<int>(type: "integer", nullable: false),
                    ClientStatus = table.Column<int>(type: "integer", nullable: false),
                    VkAccId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VkClients", x => x.VkClientId);
                    table.ForeignKey(
                        name: "FK_VkClients_VkAccs_VkAccId",
                        column: x => x.VkAccId,
                        principalTable: "VkAccs",
                        principalColumn: "VkAccId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VkAccs_UserId",
                table: "VkAccs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VkClients_VkAccId",
                table: "VkClients",
                column: "VkAccId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VkClients");

            migrationBuilder.DropTable(
                name: "VkAccs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
