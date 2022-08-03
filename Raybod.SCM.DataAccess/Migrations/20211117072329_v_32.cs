using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_32 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationComments",
                columns: table => new
                {
                    OperationCommentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    OperationId = table.Column<Guid>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ParentCommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationComments", x => x.OperationCommentId);
                    table.ForeignKey(
                        name: "FK_OperationComments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationComments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationComments_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationComments_OperationComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "OperationComments",
                        principalColumn: "OperationCommentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationAttachments",
                columns: table => new
                {
                    OperationAttachmentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    OperationId = table.Column<Guid>(nullable: true),
                    OperationCommentId = table.Column<long>(nullable: true),
                    FileName = table.Column<string>(maxLength: 250, nullable: true),
                    FileSrc = table.Column<string>(maxLength: 250, nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationAttachments", x => x.OperationAttachmentId);
                    table.ForeignKey(
                        name: "FK_OperationAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationAttachments_OperationComments_OperationCommentId",
                        column: x => x.OperationCommentId,
                        principalTable: "OperationComments",
                        principalColumn: "OperationCommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationAttachments_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationCommentUsers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    OperationCommentId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationCommentUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationCommentUsers_OperationComments_OperationCommentId",
                        column: x => x.OperationCommentId,
                        principalTable: "OperationComments",
                        principalColumn: "OperationCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationCommentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationAttachments_AdderUserId",
                table: "OperationAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationAttachments_ModifierUserId",
                table: "OperationAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationAttachments_OperationCommentId",
                table: "OperationAttachments",
                column: "OperationCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationAttachments_OperationId",
                table: "OperationAttachments",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationComments_AdderUserId",
                table: "OperationComments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationComments_ModifierUserId",
                table: "OperationComments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationComments_OperationId",
                table: "OperationComments",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationComments_ParentCommentId",
                table: "OperationComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCommentUsers_OperationCommentId",
                table: "OperationCommentUsers",
                column: "OperationCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationCommentUsers_UserId",
                table: "OperationCommentUsers",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationAttachments");

            migrationBuilder.DropTable(
                name: "OperationCommentUsers");

            migrationBuilder.DropTable(
                name: "OperationComments");
        }
    }
}
