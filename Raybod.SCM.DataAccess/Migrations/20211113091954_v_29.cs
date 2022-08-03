using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_29 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    OperationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    OperationGroupId = table.Column<int>(nullable: false),
                    OperationDescription = table.Column<string>(maxLength: 250, nullable: false),
                    OperationCode = table.Column<string>(maxLength: 64, nullable: false),
                    OperationStatus = table.Column<int>(nullable: false),
                    AreaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.OperationId);
                    table.ForeignKey(
                        name: "FK_Operations_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_OperationGroups_OperationGroupId",
                        column: x => x.OperationGroupId,
                        principalTable: "OperationGroups",
                        principalColumn: "OperationGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AdderUserId",
                table: "Operations",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AreaId",
                table: "Operations",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ContractCode",
                table: "Operations",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ModifierUserId",
                table: "Operations",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_OperationGroupId",
                table: "Operations",
                column: "OperationGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");
        }
    }
}
