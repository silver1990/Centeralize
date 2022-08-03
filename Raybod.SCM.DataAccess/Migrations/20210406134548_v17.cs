using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractFormConfigs",
                columns: table => new
                {
                    ContractFormConfigId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    FormName = table.Column<int>(nullable: false),
                    CodingType = table.Column<int>(nullable: false),
                    FixedPart = table.Column<string>(nullable: true),
                    LengthOfSequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractFormConfigs", x => x.ContractFormConfigId);
                    table.ForeignKey(
                        name: "FK_ContractFormConfigs_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractFormConfigs_ContractCode",
                table: "ContractFormConfigs",
                column: "ContractCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractFormConfigs");
        }
    }
}
