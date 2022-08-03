using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductGroupId",
                table: "PurchaseRequests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_ProductGroupId",
                table: "PurchaseRequests",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_ProductGroups_ProductGroupId",
                table: "PurchaseRequests",
                column: "ProductGroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_ProductGroups_ProductGroupId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_ProductGroupId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "PurchaseRequests");
        }
    }
}
