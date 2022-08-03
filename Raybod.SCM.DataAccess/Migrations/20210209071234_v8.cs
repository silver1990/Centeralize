using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "POCommentId",
                table: "PAttachments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "POComments",
                columns: table => new
                {
                    POCommentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    POId = table.Column<long>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ParentCommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POComments", x => x.POCommentId);
                    table.ForeignKey(
                        name: "FK_POComments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POComments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POComments_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POComments_POComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "POComments",
                        principalColumn: "POCommentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "POCommentUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    POCommentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POCommentUsers", x => new { x.UserId, x.POCommentId });
                    table.ForeignKey(
                        name: "FK_POCommentUsers_POComments_POCommentId",
                        column: x => x.POCommentId,
                        principalTable: "POComments",
                        principalColumn: "POCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POCommentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_POCommentId",
                table: "PAttachments",
                column: "POCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_POComments_AdderUserId",
                table: "POComments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POComments_ModifierUserId",
                table: "POComments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POComments_POId",
                table: "POComments",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_POComments_ParentCommentId",
                table: "POComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_POCommentUsers_POCommentId",
                table: "POCommentUsers",
                column: "POCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PAttachments_POComments_POCommentId",
                table: "PAttachments",
                column: "POCommentId",
                principalTable: "POComments",
                principalColumn: "POCommentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAttachments_POComments_POCommentId",
                table: "PAttachments");

            migrationBuilder.DropTable(
                name: "POCommentUsers");

            migrationBuilder.DropTable(
                name: "POComments");

            migrationBuilder.DropIndex(
                name: "IX_PAttachments_POCommentId",
                table: "PAttachments");

            migrationBuilder.DropColumn(
                name: "POCommentId",
                table: "PAttachments");
        }
    }
}
