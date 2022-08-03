using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateSeen",
                table: "UserNotifications",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "UserNotifications",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateSeen",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "UserNotifications");
        }
    }
}
