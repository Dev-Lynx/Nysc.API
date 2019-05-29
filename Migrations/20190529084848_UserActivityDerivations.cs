using Microsoft.EntityFrameworkCore.Migrations;

namespace Nysc.API.Migrations
{
    public partial class UserActivityDerivations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoordinatorId",
                table: "Activities",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Activities",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CoordinatorId",
                table: "Activities",
                column: "CoordinatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_AspNetUsers_CoordinatorId",
                table: "Activities",
                column: "CoordinatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_AspNetUsers_CoordinatorId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_CoordinatorId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "CoordinatorId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Activities");
        }
    }
}
