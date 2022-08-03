using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Addresses_AddressId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_AddressId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Warehouses");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Warehouses",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Warehouses",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Warehouses");

            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Warehouses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Warehouses",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AddressId",
                table: "Warehouses",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Addresses_AddressId",
                table: "Warehouses",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
