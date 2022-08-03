using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_33 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationActivities",
                columns: table => new
                {
                    OperationActivityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationId = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    OperationActivityStatus = table.Column<int>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    ActivityOwnerId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Duration = table.Column<double>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ProgressPercent = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationActivities", x => x.OperationActivityId);
                    table.ForeignKey(
                        name: "FK_OperationActivities_Users_ActivityOwnerId",
                        column: x => x.ActivityOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationActivities_Operations_OperationId",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "OperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationActivityTimesheets",
                columns: table => new
                {
                    ActivityTimesheetId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    OperationActivityId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    DateIssue = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationActivityTimesheets", x => x.ActivityTimesheetId);
                    table.ForeignKey(
                        name: "FK_OperationActivityTimesheets_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationActivityTimesheets_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationActivityTimesheets_OperationActivities_OperationActivityId",
                        column: x => x.OperationActivityId,
                        principalTable: "OperationActivities",
                        principalColumn: "OperationActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivities_ActivityOwnerId",
                table: "OperationActivities",
                column: "ActivityOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivities_OperationId",
                table: "OperationActivities",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivityTimesheets_AdderUserId",
                table: "OperationActivityTimesheets",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivityTimesheets_ModifierUserId",
                table: "OperationActivityTimesheets",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationActivityTimesheets_OperationActivityId",
                table: "OperationActivityTimesheets",
                column: "OperationActivityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationActivityTimesheets");

            migrationBuilder.DropTable(
                name: "OperationActivities");
        }
    }
}
