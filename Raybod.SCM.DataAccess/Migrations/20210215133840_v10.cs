using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RFPStatusLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
                    DateIssued = table.Column<DateTime>(nullable: false),
                    RFPId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPStatusLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPStatusLogs_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RFPStatusLogs_RFPId",
                table: "RFPStatusLogs",
                column: "RFPId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RFPStatusLogs");
        }
    }
}
