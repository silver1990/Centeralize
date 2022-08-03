using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_42 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "fileDriveFiles",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DirectoryName",
                table: "FileDriveDirectories",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateIssued",
                table: "Contracts",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEnd",
                table: "Contracts",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEffective",
                table: "Contracts",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Contracts",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "Contracts",
                type: "decimal(18, 2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 2)");

            migrationBuilder.AlterColumn<string>(
                name: "ContractNumber",
                table: "Contracts",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<int>(
                name: "ContractDuration",
                table: "Contracts",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            

            migrationBuilder.CreateTable(
                name: "fileDriveShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    EntityType = table.Column<int>(nullable: false),
                    Accessablity = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileDriveShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fileDriveShares_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fileDriveShares_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

           

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveShares_AdderUserId",
                table: "fileDriveShares",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveShares_ModifierUserId",
                table: "fileDriveShares",
                column: "ModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles");

            migrationBuilder.DropTable(
                name: "fileDriveShares");

            migrationBuilder.DropTable(
                name: "PlanServices");

           

           

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "fileDriveFiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DirectoryName",
                table: "FileDriveDirectories",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateIssued",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEnd",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateEffective",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Contracts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "Contracts",
                type: "decimal(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18, 2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContractNumber",
                table: "Contracts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContractDuration",
                table: "Contracts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
