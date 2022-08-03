using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_43 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "fileDriveShares");

            migrationBuilder.AddColumn<Guid>(
                name: "DirectoryId",
                table: "fileDriveShares",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "fileDriveShares",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveShares_DirectoryId",
                table: "fileDriveShares",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveShares_FileId",
                table: "fileDriveShares",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_fileDriveShares_FileDriveDirectories_DirectoryId",
                table: "fileDriveShares",
                column: "DirectoryId",
                principalTable: "FileDriveDirectories",
                principalColumn: "DirectoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fileDriveShares_fileDriveFiles_FileId",
                table: "fileDriveShares",
                column: "FileId",
                principalTable: "fileDriveFiles",
                principalColumn: "FileId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fileDriveShares_FileDriveDirectories_DirectoryId",
                table: "fileDriveShares");

            migrationBuilder.DropForeignKey(
                name: "FK_fileDriveShares_fileDriveFiles_FileId",
                table: "fileDriveShares");

            migrationBuilder.DropIndex(
                name: "IX_fileDriveShares_DirectoryId",
                table: "fileDriveShares");

            migrationBuilder.DropIndex(
                name: "IX_fileDriveShares_FileId",
                table: "fileDriveShares");

            migrationBuilder.DropColumn(
                name: "DirectoryId",
                table: "fileDriveShares");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "fileDriveShares");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "fileDriveShares",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
