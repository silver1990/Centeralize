using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class cleardatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyUsers_Consultings_ConsultingId",
                table: "CompanyUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Consultings_ConsultingId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentCommunications_Customers_CustomerId",
                table: "DocumentCommunications");

            migrationBuilder.DropTable(
                name: "Consultings");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ConsultingId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_ConsultingId",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "ConsultingId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ConsultingId",
                table: "CompanyUsers");

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "Transmittals",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "DocumentTQNCRs",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "DocumentCommunications",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompanyIssue",
                table: "DocumentCommunications",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "DocumentCommunications",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "Contracts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "CompanyUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consultants",
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
                    ConsultantCode = table.Column<string>(maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_Consultants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultants_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consultants_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_ConsultantId",
                table: "Transmittals",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_ConsultantId",
                table: "DocumentTQNCRs",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_ConsultantId",
                table: "DocumentCommunications",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ConsultantId",
                table: "Contracts",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_ConsultantId",
                table: "CompanyUsers",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultants_AdderUserId",
                table: "Consultants",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultants_ModifierUserId",
                table: "Consultants",
                column: "ModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyUsers_Consultants_ConsultantId",
                table: "CompanyUsers",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Consultants_ConsultantId",
                table: "Contracts",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentCommunications_Consultants_ConsultantId",
                table: "DocumentCommunications",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentCommunications_Customers_CustomerId",
                table: "DocumentCommunications",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTQNCRs_Consultants_ConsultantId",
                table: "DocumentTQNCRs",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transmittals_Consultants_ConsultantId",
                table: "Transmittals",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyUsers_Consultants_ConsultantId",
                table: "CompanyUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Consultants_ConsultantId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentCommunications_Consultants_ConsultantId",
                table: "DocumentCommunications");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentCommunications_Customers_CustomerId",
                table: "DocumentCommunications");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTQNCRs_Consultants_ConsultantId",
                table: "DocumentTQNCRs");

            migrationBuilder.DropForeignKey(
                name: "FK_Transmittals_Consultants_ConsultantId",
                table: "Transmittals");

            migrationBuilder.DropTable(
                name: "Consultants");

            migrationBuilder.DropIndex(
                name: "IX_Transmittals_ConsultantId",
                table: "Transmittals");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTQNCRs_ConsultantId",
                table: "DocumentTQNCRs");

            migrationBuilder.DropIndex(
                name: "IX_DocumentCommunications_ConsultantId",
                table: "DocumentCommunications");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ConsultantId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyUsers_ConsultantId",
                table: "CompanyUsers");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "Transmittals");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "DocumentTQNCRs");

            migrationBuilder.DropColumn(
                name: "CompanyIssue",
                table: "DocumentCommunications");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "DocumentCommunications");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "CompanyUsers");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "DocumentCommunications",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsultingId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsultingId",
                table: "CompanyUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consultings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdderUserId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ConsultingCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ModifierUserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    TellPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
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

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentCommunications_Customers_CustomerId",
                table: "DocumentCommunications",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
