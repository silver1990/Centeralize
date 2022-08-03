using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PRContracts_RFPs_RFPId",
                table: "PRContracts");

            migrationBuilder.DropIndex(
                name: "IX_PRContracts_RFPId",
                table: "PRContracts");

            migrationBuilder.DropColumn(
                name: "IsParallel",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "IsSendSmsNotification",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "SCMFormPermission",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "SCMModule",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "SCMWorkFlow",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "WorkFlowStateId",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "IsParallel",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsSendSmsNotification",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "SCMFormPermission",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "SCMModule",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "SCMWorkFlow",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "WorkFlowStateId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RFPId",
                table: "PRContracts");

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobalGroup",
                table: "TeamWorkUserRoles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobalGroup",
                table: "Roles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "RFPItemId",
                table: "PRContractSubjects",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ProductGroupId",
                table: "PRContracts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjects_RFPItemId",
                table: "PRContractSubjects",
                column: "RFPItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_ProductGroupId",
                table: "PRContracts",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_PRContracts_ProductGroups_ProductGroupId",
                table: "PRContracts",
                column: "ProductGroupId",
                principalTable: "ProductGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PRContractSubjects_RFPItems_RFPItemId",
                table: "PRContractSubjects",
                column: "RFPItemId",
                principalTable: "RFPItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PRContracts_ProductGroups_ProductGroupId",
                table: "PRContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_PRContractSubjects_RFPItems_RFPItemId",
                table: "PRContractSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PRContractSubjects_RFPItemId",
                table: "PRContractSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PRContracts_ProductGroupId",
                table: "PRContracts");

            migrationBuilder.DropColumn(
                name: "IsGlobalGroup",
                table: "TeamWorkUserRoles");

            migrationBuilder.DropColumn(
                name: "IsGlobalGroup",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RFPItemId",
                table: "PRContractSubjects");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "PRContracts");

            migrationBuilder.AddColumn<bool>(
                name: "IsParallel",
                table: "TeamWorkUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSendSmsNotification",
                table: "TeamWorkUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SCMFormPermission",
                table: "TeamWorkUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SCMModule",
                table: "TeamWorkUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SCMWorkFlow",
                table: "TeamWorkUserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkFlowStateId",
                table: "TeamWorkUserRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsParallel",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSendSmsNotification",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SCMFormPermission",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SCMModule",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SCMWorkFlow",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkFlowStateId",
                table: "Roles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RFPId",
                table: "PRContracts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_RFPId",
                table: "PRContracts",
                column: "RFPId");

            migrationBuilder.AddForeignKey(
                name: "FK_PRContracts_RFPs_RFPId",
                table: "PRContracts",
                column: "RFPId",
                principalTable: "RFPs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
