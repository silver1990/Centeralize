using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_31 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperationStatus",
                table: "OperationProgresses");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Operations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperationStatuses",
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
                    OperationStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationStatuses_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationStatuses_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationStatuses_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationStatuses_AdderUserId",
                table: "OperationStatuses",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationStatuses_ModifierUserId",
                table: "OperationStatuses",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationStatuses_OperationId",
                table: "OperationStatuses",
                column: "OperationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationStatuses");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Operations");

            migrationBuilder.AddColumn<int>(
                name: "OperationStatus",
                table: "OperationProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
