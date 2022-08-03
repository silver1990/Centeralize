using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_46 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractCode",
                table: "Products",
                type: "varchar(60)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BomType",
                table: "BomProducts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ContractCode",
                table: "Products",
                column: "ContractCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Contracts_ContractCode",
                table: "Products",
                column: "ContractCode",
                principalTable: "Contracts",
                principalColumn: "ContractCode",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Contracts_ContractCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ContractCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ContractCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BomType",
                table: "BomProducts");
        }
    }
}
