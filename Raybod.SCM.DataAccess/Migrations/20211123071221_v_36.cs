using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_36 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "OperationActivities");

            migrationBuilder.AlterColumn<double>(
                name: "ProgressPercent",
                table: "OperationProgresses",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "OperationActivities",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "TeamWorkUserOperationGroups",
                columns: table => new
                {
                    OperationGroupId = table.Column<int>(nullable: false),
                    TeamWorkUserId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUserOperationGroups", x => new { x.TeamWorkUserId, x.OperationGroupId });
                    table.ForeignKey(
                        name: "FK_TeamWorkUserOperationGroups_OperationGroups_OperationGroupId",
                        column: x => x.OperationGroupId,
                        principalTable: "OperationGroups",
                        principalColumn: "OperationGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserOperationGroups_TeamWorkUsers_TeamWorkUserId",
                        column: x => x.TeamWorkUserId,
                        principalTable: "TeamWorkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserOperationGroups_OperationGroupId",
                table: "TeamWorkUserOperationGroups",
                column: "OperationGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamWorkUserOperationGroups");

            migrationBuilder.AlterColumn<int>(
                name: "ProgressPercent",
                table: "OperationProgresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "OperationActivities",
                type: "int",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "OperationActivities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
