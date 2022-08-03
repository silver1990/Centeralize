using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_38 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DirectoryPath",
                table: "FileDriveDirectories",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DirectoryName",
                table: "FileDriveDirectories",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractCode",
                table: "FileDriveDirectories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDriveDirectories_ContractCode",
                table: "FileDriveDirectories",
                column: "ContractCode");

            migrationBuilder.AddForeignKey(
                name: "FK_FileDriveDirectories_Contracts_ContractCode",
                table: "FileDriveDirectories",
                column: "ContractCode",
                principalTable: "Contracts",
                principalColumn: "ContractCode",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileDriveDirectories_Contracts_ContractCode",
                table: "FileDriveDirectories");

            migrationBuilder.DropIndex(
                name: "IX_FileDriveDirectories_ContractCode",
                table: "FileDriveDirectories");

            migrationBuilder.DropColumn(
                name: "ContractCode",
                table: "FileDriveDirectories");

            migrationBuilder.AlterColumn<string>(
                name: "DirectoryPath",
                table: "FileDriveDirectories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<string>(
                name: "DirectoryName",
                table: "FileDriveDirectories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 250);
        }
    }
}
