using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_40 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fileDriveFiles",
                columns: table => new
                {
                    FileId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FileName = table.Column<string>(maxLength: 250, nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    DirectoryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileDriveFiles", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_fileDriveFiles_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fileDriveFiles_FileDriveDirectories_DirectoryId",
                        column: x => x.DirectoryId,
                        principalTable: "FileDriveDirectories",
                        principalColumn: "DirectoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fileDriveFiles_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveFiles_AdderUserId",
                table: "fileDriveFiles",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveFiles_DirectoryId",
                table: "fileDriveFiles",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_fileDriveFiles_ModifierUserId",
                table: "fileDriveFiles",
                column: "ModifierUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fileDriveFiles");
        }
    }
}
