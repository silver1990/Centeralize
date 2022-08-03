using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v_35 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "OperationActivityTimesheets",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "OperationActivityTimesheets");
        }
    }
}
