using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v24 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "Users",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ConsultingId",
                table: "Contracts",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsultingId",
                table: "CompanyUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consultings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    ConsultingCode = table.Column<string>(maxLength: 64, nullable: false),
                    Address = table.Column<string>(maxLength: 300, nullable: true),
                    TellPhone = table.Column<string>(maxLength: 100, nullable: true),
                    Fax = table.Column<string>(maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(maxLength: 12, nullable: true),
                    Website = table.Column<string>(maxLength: 300, nullable: true),
                    Email = table.Column<string>(maxLength: 300, nullable: true),
                    Logo = table.Column<string>(maxLength: 300, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultings_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consultings_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ConsultingId",
                table: "Contracts",
                column: "ConsultingId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_ConsultingId",
                table: "CompanyUsers",
                column: "ConsultingId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultings_AdderUserId",
                table: "Consultings",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultings_ModifierUserId",
                table: "Consultings",
                column: "ModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyUsers_Consultings_ConsultingId",
                table: "CompanyUsers",
                column: "ConsultingId",
                principalTable: "Consultings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Consultings_ConsultingId",
                table: "Contracts",
                column: "ConsultingId",
                principalTable: "Consultings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyUsers_Consultings_ConsultingId",
                table: "CompanyUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Consultings_ConsultingId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "Consultings");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ConsultingId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_ConsultingId",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConsultingId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ConsultingId",
                table: "CompanyUsers");
        }
    }
}
