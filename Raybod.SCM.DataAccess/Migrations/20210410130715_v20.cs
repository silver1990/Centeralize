using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDFTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractCode = table.Column<string>(nullable: true),
                    PDFTemplateType = table.Column<int>(nullable: false),
                    Section1 = table.Column<string>(nullable: true),
                    Section2 = table.Column<string>(nullable: true),
                    Section3 = table.Column<string>(nullable: true),
                    Section4 = table.Column<string>(nullable: true),
                    Section5 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PDFTemplates_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PDFTemplates_ContractCode",
                table: "PDFTemplates",
                column: "ContractCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDFTemplates");
        }
    }
}
