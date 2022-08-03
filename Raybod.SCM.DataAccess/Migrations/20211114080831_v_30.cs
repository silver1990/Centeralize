using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_30 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationStatus",
                table: "Operations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Operations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OperationProgresses",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    OperationId = table.Column<long>(nullable: false),
                    OperationStatus = table.Column<int>(nullable: false),
                    ProgressPercent = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationProgresses_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationProgresses_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationProgresses_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationProgresses_AdderUserId",
                table: "OperationProgresses",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationProgresses_ModifierUserId",
                table: "OperationProgresses",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationProgresses_OperationId",
                table: "OperationProgresses",
                column: "OperationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationProgresses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Operations");

            migrationBuilder.AddColumn<int>(
                name: "OperationStatus",
                table: "Operations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
