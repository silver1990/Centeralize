using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_37 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue2",
                table: "SCMAuditLogs",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue",
                table: "SCMAuditLogs",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "SCMAuditLogs",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperationGroupId",
                table: "SCMAuditLogs",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue2",
                table: "Notifications",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue",
                table: "Notifications",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(44)",
                oldMaxLength: 44,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "Notifications",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FileDriveDirectories",
                columns: table => new
                {
                    DirectoryId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: true),
                    DirectoryName = table.Column<string>(nullable: true),
                    DirectoryPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDriveDirectories", x => x.DirectoryId);
                    table.ForeignKey(
                        name: "FK_FileDriveDirectories_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDriveDirectories_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileDriveDirectories_FileDriveDirectories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FileDriveDirectories",
                        principalColumn: "DirectoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileDriveDirectories_AdderUserId",
                table: "FileDriveDirectories",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDriveDirectories_ModifierUserId",
                table: "FileDriveDirectories",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDriveDirectories_ParentId",
                table: "FileDriveDirectories",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDriveDirectories");

            migrationBuilder.DropColumn(
                name: "OperationGroupId",
                table: "SCMAuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue2",
                table: "SCMAuditLogs",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue",
                table: "SCMAuditLogs",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "SCMAuditLogs",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue2",
                table: "Notifications",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RootKeyValue",
                table: "Notifications",
                type: "nvarchar(44)",
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KeyValue",
                table: "Notifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
