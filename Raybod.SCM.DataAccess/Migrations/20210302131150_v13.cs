using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemainedQuantity",
                table: "PackingSubjects",
                newName: "ShortageQuantity");

            migrationBuilder.AddColumn<decimal>(
                name: "ShortageQuantity",
                table: "ReceiptSubjects",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShortageQuantity",
                table: "POSubjects",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortageQuantity",
                table: "ReceiptSubjects");

            migrationBuilder.DropColumn(
                name: "ShortageQuantity",
                table: "POSubjects");

            migrationBuilder.RenameColumn(
                name: "ShortageQuantity",
                table: "PackingSubjects",
                newName: "RemainedQuantity");
        }
    }
}
