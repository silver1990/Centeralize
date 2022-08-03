using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class addusertofiledrive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "fileDriveFiles",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "FileDriveDirectories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveFiles_UserId",
                table: "fileDriveFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDriveDirectories_UserId",
                table: "FileDriveDirectories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileDriveDirectories_Users_UserId",
                table: "FileDriveDirectories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileDriveDirectories_Users_UserId",
                table: "FileDriveDirectories");

            migrationBuilder.DropForeignKey(
                name: "FK_fileDriveFiles_Users_UserId",
                table: "fileDriveFiles");

            migrationBuilder.DropIndex(
                name: "IX_fileDriveFiles_UserId",
                table: "fileDriveFiles");

            migrationBuilder.DropIndex(
                name: "IX_FileDriveDirectories_UserId",
                table: "FileDriveDirectories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "fileDriveFiles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FileDriveDirectories");
        }
    }
}
