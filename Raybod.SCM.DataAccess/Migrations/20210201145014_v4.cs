using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductGroupId",
                table: "RFPs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RFPs_ProductGroupId",
                table: "RFPs",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_RFPs_ProductGroups_ProductGroupId",
                table: "RFPs",
                column: "ProductGroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RFPs_ProductGroups_ProductGroupId",
                table: "RFPs");

            migrationBuilder.DropIndex(
                name: "IX_RFPs_ProductGroupId",
                table: "RFPs");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "RFPs");
        }
    }
}
