using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "ContractAttachments");

            migrationBuilder.AddColumn<string>(
                name: "FileSrc",
                table: "RFPAttachments",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileSrc",
                table: "PaymentAttachments",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileSrc",
                table: "PAttachments",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ContractAttachments",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<string>(
                name: "FileSrc",
                table: "ContractAttachments",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSrc",
                table: "RFPAttachments");

            migrationBuilder.DropColumn(
                name: "FileSrc",
                table: "PaymentAttachments");

            migrationBuilder.DropColumn(
                name: "FileSrc",
                table: "PAttachments");

            migrationBuilder.DropColumn(
                name: "FileSrc",
                table: "ContractAttachments");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ContractAttachments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ContractAttachments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
