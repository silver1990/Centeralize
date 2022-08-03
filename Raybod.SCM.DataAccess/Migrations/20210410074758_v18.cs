using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentClass",
                table: "Transmittals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "MasterMRId",
                table: "MrpItems",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_MrpItems_MasterMRId",
                table: "MrpItems",
                column: "MasterMRId");

            migrationBuilder.AddForeignKey(
                name: "FK_MrpItems_MasterMRs_MasterMRId",
                table: "MrpItems",
                column: "MasterMRId",
                principalTable: "MasterMRs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MrpItems_MasterMRs_MasterMRId",
                table: "MrpItems");

            migrationBuilder.DropIndex(
                name: "IX_MrpItems_MasterMRId",
                table: "MrpItems");

            migrationBuilder.DropColumn(
                name: "DocumentClass",
                table: "Transmittals");

            migrationBuilder.DropColumn(
                name: "MasterMRId",
                table: "MrpItems");
        }
    }
}
