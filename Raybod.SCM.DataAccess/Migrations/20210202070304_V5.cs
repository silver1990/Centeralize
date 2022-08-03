using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class V5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetingSubjects");

            migrationBuilder.DropTable(
                name: "Budgetings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgetings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdderUserId = table.Column<int>(type: "int", nullable: true),
                    BudgetCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifierUserId = table.Column<int>(type: "int", nullable: true),
                    MrpCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MrpDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MrpId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgetings_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgetings_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgetings_Mrps_MrpId",
                        column: x => x.MrpId,
                        principalTable: "Mrps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetingSubjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdderUserId = table.Column<int>(type: "int", nullable: true),
                    BudgetingId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSettlement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifierUserId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quntity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    SettlementType = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetingSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Budgetings_BudgetingId",
                        column: x => x.BudgetingId,
                        principalTable: "Budgetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_AdderUserId",
                table: "Budgetings",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_BudgetCode",
                table: "Budgetings",
                column: "BudgetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_ModifierUserId",
                table: "Budgetings",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_MrpId",
                table: "Budgetings",
                column: "MrpId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_AdderUserId",
                table: "BudgetingSubjects",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_BudgetingId",
                table: "BudgetingSubjects",
                column: "BudgetingId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_ModifierUserId",
                table: "BudgetingSubjects",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_ProductId",
                table: "BudgetingSubjects",
                column: "ProductId");
        }
    }
}
