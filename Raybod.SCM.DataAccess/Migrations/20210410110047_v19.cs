using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentClass",
                table: "Transmittals");

            migrationBuilder.AddColumn<int>(
                name: "POI",
                table: "TransmittalRevisions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "POI",
                table: "TransmittalRevisions");

            migrationBuilder.AddColumn<int>(
                name: "DocumentClass",
                table: "Transmittals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
