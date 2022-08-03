using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_34 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdderUserId",
                table: "OperationActivities",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "OperationActivities",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifierUserId",
                table: "OperationActivities",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "OperationActivities",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivities_AdderUserId",
                table: "OperationActivities",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivities_ModifierUserId",
                table: "OperationActivities",
                column: "ModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationActivities_Users_AdderUserId",
                table: "OperationActivities",
                column: "AdderUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OperationActivities_Users_ModifierUserId",
                table: "OperationActivities",
                column: "ModifierUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationActivities_Users_AdderUserId",
                table: "OperationActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_OperationActivities_Users_ModifierUserId",
                table: "OperationActivities");

            migrationBuilder.DropIndex(
                name: "IX_OperationActivities_AdderUserId",
                table: "OperationActivities");

            migrationBuilder.DropIndex(
                name: "IX_OperationActivities_ModifierUserId",
                table: "OperationActivities");

            migrationBuilder.DropColumn(
                name: "AdderUserId",
                table: "OperationActivities");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "OperationActivities");

            migrationBuilder.DropColumn(
                name: "ModifierUserId",
                table: "OperationActivities");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "OperationActivities");
        }
    }
}
