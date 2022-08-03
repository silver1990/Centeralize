using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class V6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductGroupId",
                table: "POs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_POs_ProductGroupId",
                table: "POs",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_POs_ProductGroups_ProductGroupId",
                table: "POs",
                column: "ProductGroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POs_ProductGroups_ProductGroupId",
                table: "POs");

            migrationBuilder.DropIndex(
                name: "IX_POs_ProductGroupId",
                table: "POs");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "POs");
        }
    }
}
