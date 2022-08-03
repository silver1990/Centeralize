using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Suppliers",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EconomicCode",
                table: "Suppliers",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(12)",
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "Documents",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequiredTransmittal",
                table: "Documents",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    AreaId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    AreaTitle = table.Column<string>(maxLength: 200, nullable: false),
                    ContractCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.AreaId);
                    table.ForeignKey(
                        name: "FK_Areas_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Areas_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Areas_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AreaId",
                table: "Documents",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_AdderUserId",
                table: "Areas",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_ContractCode",
                table: "Areas",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_ModifierUserId",
                table: "Areas",
                column: "ModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Areas_AreaId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Documents_AreaId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsRequiredTransmittal",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Suppliers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EconomicCode",
                table: "Suppliers",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
