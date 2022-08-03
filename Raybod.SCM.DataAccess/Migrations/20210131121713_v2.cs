using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductGroupId",
                table: "Mrps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Mrps_ProductGroupId",
                table: "Mrps",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mrps_ProductGroups_ProductGroupId",
                table: "Mrps",
                column: "ProductGroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mrps_ProductGroups_ProductGroupId",
                table: "Mrps");

            migrationBuilder.DropIndex(
                name: "IX_Mrps_ProductGroupId",
                table: "Mrps");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "Mrps");
        }
    }
}
