using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PAttachments_POPreparations_POPreparationId",
                table: "PAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityControls_POPreparations_POPreparationId",
                table: "QualityControls");

            migrationBuilder.DropTable(
                name: "POPreparations");

            migrationBuilder.DropIndex(
                name: "IX_QualityControls_POPreparationId",
                table: "QualityControls");

            migrationBuilder.DropIndex(
                name: "IX_PAttachments_POPreparationId",
                table: "PAttachments");

            migrationBuilder.DropColumn(
                name: "POPreparationId",
                table: "QualityControls");

            migrationBuilder.DropColumn(
                name: "POPreparationId",
                table: "PAttachments");

            migrationBuilder.CreateTable(
                name: "POActivities",
                columns: table => new
                {
                    POActivityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    ActivityStatus = table.Column<int>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    ActivityOwnerId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Duration = table.Column<double>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POActivities", x => x.POActivityId);
                    table.ForeignKey(
                        name: "FK_POActivities_Users_ActivityOwnerId",
                        column: x => x.ActivityOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POActivities_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POActivityTimesheets",
                columns: table => new
                {
                    ActivityTimesheetId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    POActivityId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    DateIssue = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POActivityTimesheets", x => x.ActivityTimesheetId);
                    table.ForeignKey(
                        name: "FK_POActivityTimesheets_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POActivityTimesheets_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POActivityTimesheets_POActivities_POActivityId",
                        column: x => x.POActivityId,
                        principalTable: "POActivities",
                        principalColumn: "POActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POActivities_ActivityOwnerId",
                table: "POActivities",
                column: "ActivityOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_POActivities_POId",
                table: "POActivities",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_POActivityTimesheets_AdderUserId",
                table: "POActivityTimesheets",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POActivityTimesheets_ModifierUserId",
                table: "POActivityTimesheets",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POActivityTimesheets_POActivityId",
                table: "POActivityTimesheets",
                column: "POActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POActivityTimesheets");

            migrationBuilder.DropTable(
                name: "POActivities");

            migrationBuilder.AddColumn<long>(
                name: "POPreparationId",
                table: "QualityControls",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "POPreparationId",
                table: "PAttachments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "POPreparations",
                columns: table => new
                {
                    POPreparationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdderUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFinished = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    HasQualityControl = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsQualityControlDone = table.Column<bool>(type: "bit", nullable: false),
                    ModifierUserId = table.Column<int>(type: "int", nullable: true),
                    POId = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    SubmitProgress = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18, 4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POPreparations", x => x.POPreparationId);
                    table.ForeignKey(
                        name: "FK_POPreparations_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POPreparations_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POPreparations_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_POPreparationId",
                table: "QualityControls",
                column: "POPreparationId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_POPreparationId",
                table: "PAttachments",
                column: "POPreparationId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_AdderUserId",
                table: "POPreparations",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_ModifierUserId",
                table: "POPreparations",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_POId",
                table: "POPreparations",
                column: "POId");

            migrationBuilder.AddForeignKey(
                name: "FK_PAttachments_POPreparations_POPreparationId",
                table: "PAttachments",
                column: "POPreparationId",
                principalTable: "POPreparations",
                principalColumn: "POPreparationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityControls_POPreparations_POPreparationId",
                table: "QualityControls",
                column: "POPreparationId",
                principalTable: "POPreparations",
                principalColumn: "POPreparationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
