using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v23 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Documents",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "BomProducts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BomProducts_AreaId",
                table: "BomProducts",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_BomProducts_Areas_AreaId",
                table: "BomProducts",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BomProducts_Areas_AreaId",
                table: "BomProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_BomProducts_AreaId",
                table: "BomProducts");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "BomProducts");

            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Documents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
