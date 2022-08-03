using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Raybod.SCM.DataAccess.Migrations
{
    public partial class v1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(maxLength: 60, nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(maxLength: 200, nullable: true),
                    EconomicCode = table.Column<string>(nullable: true),
                    RegistrationCode = table.Column<string>(nullable: true),
                    NationalId = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Website = table.Column<string>(maxLength: 300, nullable: true),
                    Address = table.Column<string>(maxLength: 300, nullable: true),
                    TellPhone = table.Column<string>(maxLength: 20, nullable: true),
                    Fax = table.Column<string>(maxLength: 20, nullable: true),
                    Email = table.Column<string>(maxLength: 300, nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Logo = table.Column<string>(maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnits",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Unit = table.Column<string>(maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 150, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 150, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    IsParallel = table.Column<bool>(nullable: false),
                    WorkFlowStateId = table.Column<int>(nullable: true),
                    SCMWorkFlow = table.Column<int>(nullable: false),
                    SCMModule = table.Column<int>(nullable: false),
                    SCMFormPermission = table.Column<int>(nullable: false),
                    SubModuleName = table.Column<string>(maxLength: 250, nullable: false),
                    SCMEvents = table.Column<string>(nullable: true),
                    IsSendNotification = table.Column<bool>(nullable: false),
                    IsSendMailNotification = table.Column<bool>(nullable: false),
                    IsSendSmsNotification = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 100, nullable: false),
                    FullName = table.Column<string>(maxLength: 150, nullable: true),
                    Mobile = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 50, nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Telephone = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Image = table.Column<string>(maxLength: 300, nullable: true),
                    DateExpireRefreshToken = table.Column<DateTime>(nullable: true),
                    RefreshToken = table.Column<string>(maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeliveryLocation = table.Column<string>(maxLength: 250, nullable: true),
                    Phone = table.Column<string>(maxLength: 14, nullable: true),
                    AddressType = table.Column<int>(nullable: false),
                    Country = table.Column<string>(maxLength: 150, nullable: false),
                    Province = table.Column<string>(maxLength: 150, nullable: false),
                    City = table.Column<string>(maxLength: 150, nullable: false),
                    StreetAddress = table.Column<string>(maxLength: 800, nullable: false),
                    PostalCode = table.Column<string>(maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    CustomerCode = table.Column<string>(maxLength: 64, nullable: false),
                    Address = table.Column<string>(maxLength: 300, nullable: false),
                    TellPhone = table.Column<string>(maxLength: 100, nullable: true),
                    Fax = table.Column<string>(maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(maxLength: 12, nullable: true),
                    Website = table.Column<string>(maxLength: 300, nullable: true),
                    Email = table.Column<string>(maxLength: 300, nullable: true),
                    Logo = table.Column<string>(maxLength: 300, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentGroups",
                columns: table => new
                {
                    DocumentGroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentGroupCode = table.Column<string>(maxLength: 64, nullable: false),
                    Title = table.Column<string>(maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentGroups", x => x.DocumentGroupId);
                    table.ForeignKey(
                        name: "FK_DocumentGroups_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentGroups_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    BaseContratcCode = table.Column<string>(nullable: true),
                    PerformerUserId = table.Column<int>(nullable: false),
                    DateCreate = table.Column<DateTime>(nullable: false),
                    DateDone = table.Column<DateTime>(nullable: true),
                    NotifEvent = table.Column<int>(nullable: false),
                    FormCode = table.Column<string>(maxLength: 64, nullable: true),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    Quantity = table.Column<string>(maxLength: 250, nullable: true),
                    Message = table.Column<string>(maxLength: 800, nullable: true),
                    KeyValue = table.Column<string>(maxLength: 20, nullable: true),
                    RootKeyValue = table.Column<string>(maxLength: 44, nullable: true),
                    RootKeyValue2 = table.Column<string>(maxLength: 44, nullable: true),
                    Temp = table.Column<string>(maxLength: 250, nullable: true),
                    IsDone = table.Column<bool>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_PerformerUserId",
                        column: x => x.PerformerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductGroupCode = table.Column<string>(maxLength: 60, nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductGroups_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductGroups_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(maxLength: 400, nullable: false),
                    ServiceCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    SupplierCode = table.Column<string>(maxLength: 60, nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    TellPhone = table.Column<string>(maxLength: 20, nullable: true),
                    Fax = table.Column<string>(maxLength: 20, nullable: true),
                    Address = table.Column<string>(maxLength: 300, nullable: true),
                    PostalCode = table.Column<string>(maxLength: 10, nullable: true),
                    EconomicCode = table.Column<string>(maxLength: 12, nullable: true),
                    NationalId = table.Column<string>(maxLength: 11, nullable: true),
                    Website = table.Column<string>(maxLength: 300, nullable: true),
                    Email = table.Column<string>(maxLength: 300, nullable: true),
                    Logo = table.Column<string>(maxLength: 300, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suppliers_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInvisibleTeamWorks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamWorkId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvisibleTeamWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInvisibleTeamWorks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    WarehouseCode = table.Column<string>(maxLength: 60, nullable: true),
                    Description = table.Column<string>(maxLength: 800, nullable: true),
                    AddressId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Warehouses_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warehouses_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractNumber = table.Column<string>(maxLength: 64, nullable: false),
                    Description = table.Column<string>(maxLength: 800, nullable: true),
                    ParnetContractCode = table.Column<string>(nullable: true),
                    Details = table.Column<string>(nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ContractStatus = table.Column<int>(nullable: false),
                    ContractType = table.Column<int>(nullable: false),
                    DateIssued = table.Column<DateTime>(nullable: false),
                    DateEffective = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    ContractDuration = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractCode);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Contracts_ParnetContractCode",
                        column: x => x.ParnetContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationWorkFlowTemplates",
                columns: table => new
                {
                    ConfirmationWorkFlowTemplateId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentGroupId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationWorkFlowTemplates", x => x.ConfirmationWorkFlowTemplateId);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowTemplates_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowTemplates_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowTemplates_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    NotificationId = table.Column<Guid>(nullable: false),
                    UserNotificationsId = table.Column<long>(nullable: false),
                    IsUserSetTaskDone = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => new { x.UserId, x.NotificationId });
                    table.ForeignKey(
                        name: "FK_UserNotifications_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductGroupId = table.Column<int>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 60, nullable: false),
                    TechnicalNumber = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 400, nullable: false),
                    Image = table.Column<string>(maxLength: 300, nullable: true),
                    Unit = table.Column<string>(maxLength: 100, nullable: false),
                    ProductType = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyUsers",
                columns: table => new
                {
                    CompanyUserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false),
                    LastName = table.Column<string>(maxLength: 100, nullable: true),
                    Email = table.Column<string>(nullable: true),
                    CustomerId = table.Column<int>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUsers", x => x.CompanyUserId);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProductGroups",
                columns: table => new
                {
                    SupplierId = table.Column<int>(nullable: false),
                    ProductGroupId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProductGroups", x => new { x.SupplierId, x.ProductGroupId });
                    table.ForeignKey(
                        name: "FK_SupplierProductGroups_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierProductGroups_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractAddresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAddresses", x => new { x.AddressId, x.ContractCode });
                    table.ForeignKey(
                        name: "FK_ContractAddresses_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractAddresses_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(maxLength: 300, nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractDocumentGroupLists",
                columns: table => new
                {
                    ContractDocumentGroupListId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    DocumentGroupId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocumentGroupLists", x => x.ContractDocumentGroupListId);
                    table.ForeignKey(
                        name: "FK_ContractDocumentGroupLists_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractDocumentGroupLists_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractDocumentGroupLists_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractDocumentGroupLists_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mrps",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    MrpNumber = table.Column<string>(maxLength: 64, nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: false),
                    MrpStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mrps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mrps_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mrps_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Mrps_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PaymentNumber = table.Column<string>(maxLength: 64, nullable: false),
                    ContractCode = table.Column<string>(maxLength: 60, nullable: true),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    SupplierId = table.Column<int>(nullable: false),
                    CurrencyType = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPNumber = table.Column<string>(maxLength: 64, nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    DateDue = table.Column<DateTime>(nullable: false),
                    RFPType = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPs_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SCMAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    BaseContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    PerformerUserId = table.Column<int>(nullable: true),
                    DateCreate = table.Column<DateTime>(nullable: false),
                    NotifEvent = table.Column<int>(nullable: false),
                    Message = table.Column<string>(maxLength: 800, nullable: true),
                    KeyValue = table.Column<string>(maxLength: 44, nullable: true),
                    RootKeyValue = table.Column<string>(maxLength: 44, nullable: true),
                    RootKeyValue2 = table.Column<string>(maxLength: 44, nullable: true),
                    FormCode = table.Column<string>(maxLength: 64, nullable: true),
                    Description = table.Column<string>(maxLength: 250, nullable: true),
                    Quantity = table.Column<string>(maxLength: 250, nullable: true),
                    Temp = table.Column<string>(maxLength: 250, nullable: true),
                    ProductGroupId = table.Column<int>(nullable: true),
                    DocumentGroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCMAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SCMAuditLogs_Contracts_BaseContractCode",
                        column: x => x.BaseContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCMAuditLogs_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCMAuditLogs_Users_PerformerUserId",
                        column: x => x.PerformerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SCMAuditLogs_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(maxLength: 800, nullable: true),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DateCreat = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamWorks_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transmittals",
                columns: table => new
                {
                    TransmittalId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    DocumentGroupId = table.Column<int>(nullable: false),
                    TransmittalType = table.Column<int>(nullable: false),
                    TransmittalNumber = table.Column<string>(maxLength: 64, nullable: true),
                    Description = table.Column<string>(maxLength: 800, nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    FullName = table.Column<string>(maxLength: 200, nullable: true),
                    Email = table.Column<string>(maxLength: 300, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transmittals", x => x.TransmittalId);
                    table.ForeignKey(
                        name: "FK_Transmittals_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transmittals_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transmittals_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transmittals_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transmittals_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationWorkFlowTemplateUsers",
                columns: table => new
                {
                    ConfirmationWorkFlowTemplateUserId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ConfirmationWorkFlowTemplateId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationWorkFlowTemplateUsers", x => x.ConfirmationWorkFlowTemplateUserId);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowTemplateUsers_ConfirmationWorkFlowTemplates_ConfirmationWorkFlowTemplateId",
                        column: x => x.ConfirmationWorkFlowTemplateId,
                        principalTable: "ConfirmationWorkFlowTemplates",
                        principalColumn: "ConfirmationWorkFlowTemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowTemplateUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    DocumentGroupId = table.Column<int>(nullable: false),
                    ConfirmationWorkFlowTemplateId = table.Column<long>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    DocNumber = table.Column<string>(maxLength: 64, nullable: false),
                    ClientDocNumber = table.Column<string>(maxLength: 100, nullable: true),
                    DocTitle = table.Column<string>(maxLength: 250, nullable: false),
                    DocRemark = table.Column<string>(maxLength: 800, nullable: true),
                    DocClass = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_ConfirmationWorkFlowTemplates_ConfirmationWorkFlowTemplateId",
                        column: x => x.ConfirmationWorkFlowTemplateId,
                        principalTable: "ConfirmationWorkFlowTemplates",
                        principalColumn: "ConfirmationWorkFlowTemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BomProducts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    ParentBomId = table.Column<long>(nullable: true),
                    CoefficientUse = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MaterialType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomProducts_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BomProducts_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BomProducts_BomProducts_ParentBomId",
                        column: x => x.ParentBomId,
                        principalTable: "BomProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BomProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    BalanceingRate = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PriceTotal = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    IsDoneMrp = table.Column<bool>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractSubjects_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSubjects_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractSubjects_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterMRs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    ProductId = table.Column<int>(nullable: false),
                    BomProductId = table.Column<int>(nullable: false),
                    GrossRequirement = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedGrossRequirement = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMRs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterMRs_Products_BomProductId",
                        column: x => x.BomProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MasterMRs_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MasterMRs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseProducts",
                columns: table => new
                {
                    WarehouseProductId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    ProductId = table.Column<int>(nullable: false),
                    Inventory = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    AcceptQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ReceiptQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseProducts", x => x.WarehouseProductId);
                    table.ForeignKey(
                        name: "FK_WarehouseProducts_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseProducts_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Budgetings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(maxLength: 300, nullable: false),
                    Description = table.Column<string>(maxLength: 800, nullable: true),
                    BudgetCode = table.Column<string>(maxLength: 60, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    MrpId = table.Column<long>(nullable: false),
                    MrpDescription = table.Column<string>(maxLength: 300, nullable: false),
                    MrpCode = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgetings_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgetings_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgetings_Mrps_MrpId",
                        column: x => x.MrpId,
                        principalTable: "Mrps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MrpItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    MrpId = table.Column<long>(nullable: false),
                    GrossRequirement = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    NetRequirement = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    WarehouseStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ReservedStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    SurplusQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FinalRequirment = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DoneStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PR = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PO = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrpItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MrpItems_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrpItems_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MrpItems_Mrps_MrpId",
                        column: x => x.MrpId,
                        principalTable: "Mrps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MrpItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequests",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    PRCode = table.Column<string>(maxLength: 64, nullable: false),
                    MrpId = table.Column<long>(nullable: true),
                    TypeOfInquiry = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    ConfirmNote = table.Column<string>(maxLength: 800, nullable: true),
                    PRStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Contracts_ContractCode",
                        column: x => x.ContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequests_Mrps_MrpId",
                        column: x => x.MrpId,
                        principalTable: "Mrps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRContracts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    RFPId = table.Column<long>(nullable: false),
                    PRContractCode = table.Column<string>(maxLength: 64, nullable: true),
                    BaseContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    SupplierId = table.Column<int>(nullable: false),
                    DeliveryLocation = table.Column<int>(nullable: false),
                    PContractType = table.Column<int>(nullable: false),
                    CurrencyType = table.Column<int>(nullable: false),
                    DateIssued = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    ContractDuration = table.Column<int>(nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FinalTotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PRContractStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ServiceId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRContracts_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRContracts_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRContracts_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRContracts_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRContracts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPInqueries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPId = table.Column<long>(nullable: false),
                    RFPInqueryType = table.Column<int>(nullable: false),
                    DefaultInquery = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPInqueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPInqueries_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPInqueries_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPInqueries_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RFPSuppliers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPId = table.Column<long>(nullable: false),
                    SupplierId = table.Column<int>(nullable: false),
                    TBEScore = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    CBEScore = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    IsWinner = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    TBENote = table.Column<string>(maxLength: 800, nullable: true),
                    CBENote = table.Column<string>(maxLength: 800, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPSuppliers_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPSuppliers_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPSuppliers_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFPSuppliers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogUserReceivers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    SCMAuditLogId = table.Column<Guid>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogUserReceivers", x => new { x.UserId, x.SCMAuditLogId });
                    table.ForeignKey(
                        name: "FK_LogUserReceivers_SCMAuditLogs_SCMAuditLogId",
                        column: x => x.SCMAuditLogId,
                        principalTable: "SCMAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogUserReceivers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSeenSCMAuditLogs",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    SCMAuditLogId = table.Column<Guid>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    DateSeen = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSeenSCMAuditLogs", x => new { x.UserId, x.SCMAuditLogId });
                    table.ForeignKey(
                        name: "FK_UserSeenSCMAuditLogs_SCMAuditLogs_SCMAuditLogId",
                        column: x => x.SCMAuditLogId,
                        principalTable: "SCMAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSeenSCMAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorkUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamWorkId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamWorkUsers_TeamWorks_TeamWorkId",
                        column: x => x.TeamWorkId,
                        principalTable: "TeamWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentProducts",
                columns: table => new
                {
                    ProductId = table.Column<int>(nullable: false),
                    DocumentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentProducts", x => new { x.ProductId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_DocumentProducts_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRevisions",
                columns: table => new
                {
                    DocumentRevisionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentId = table.Column<long>(nullable: false),
                    DocumentRevisionCode = table.Column<string>(maxLength: 64, nullable: false),
                    Description = table.Column<string>(maxLength: 800, nullable: true),
                    RevisionPageNumber = table.Column<int>(nullable: true),
                    RevisionPageSize = table.Column<string>(maxLength: 10, nullable: true),
                    IsLastConfirmRevision = table.Column<bool>(nullable: false),
                    IsLastRevision = table.Column<bool>(nullable: false),
                    RevisionStatus = table.Column<int>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRevisions", x => x.DocumentRevisionId);
                    table.ForeignKey(
                        name: "FK_DocumentRevisions_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentRevisions_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentRevisions_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetingSubjects",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    BudgetingId = table.Column<long>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    Quntity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    SettlementType = table.Column<int>(nullable: false),
                    DateSettlement = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetingSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Budgetings_BudgetingId",
                        column: x => x.BudgetingId,
                        principalTable: "Budgetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetingSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRConfirmLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsConfirm = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    PurchaseRequestId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRConfirmLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRConfirmLogs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRConfirmLogs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRConfirmLogs_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequestItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    PurchaseRequestId = table.Column<long>(nullable: false),
                    PRItemStatus = table.Column<int>(nullable: false),
                    Quntity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequestItems_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POs",
                columns: table => new
                {
                    POId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    BaseContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    POCode = table.Column<string>(nullable: true),
                    DeliveryLocation = table.Column<int>(nullable: false),
                    PORefType = table.Column<int>(nullable: false),
                    POStatus = table.Column<int>(nullable: false),
                    CurrencyType = table.Column<int>(nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PContractType = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: false),
                    DateDelivery = table.Column<DateTime>(nullable: false),
                    PRContractId = table.Column<long>(nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    FinalTotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ServiceId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POs", x => x.POId);
                    table.ForeignKey(
                        name: "FK_POs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POs_Contracts_BaseContractCode",
                        column: x => x.BaseContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POs_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POs_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POs_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRContractServices",
                columns: table => new
                {
                    PRContractId = table.Column<long>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRContractServices", x => new { x.PRContractId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_PRContractServices_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRContractServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRContractSubjects",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    PRContractId = table.Column<long>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ReservedStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedQuantityToInvoice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRContractSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRContractSubjects_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRContractSubjects_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PRContractSubjects_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRContractSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPComments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPSupplierId = table.Column<long>(nullable: false),
                    RFPInqueryType = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ParentCommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPComments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPComments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPComments_RFPComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "RFPComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPComments_RFPSuppliers_RFPSupplierId",
                        column: x => x.RFPSupplierId,
                        principalTable: "RFPSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPSupplierProposals",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPSupplierId = table.Column<long>(nullable: false),
                    RFPInqueryId = table.Column<long>(nullable: false),
                    IsAnswered = table.Column<bool>(nullable: false),
                    IsEvaluated = table.Column<bool>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    EvaluationScore = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPSupplierProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPSupplierProposals_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPSupplierProposals_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPSupplierProposals_RFPInqueries_RFPInqueryId",
                        column: x => x.RFPInqueryId,
                        principalTable: "RFPInqueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFPSupplierProposals_RFPSuppliers_RFPSupplierId",
                        column: x => x.RFPSupplierId,
                        principalTable: "RFPSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorkUserDocumentGroups",
                columns: table => new
                {
                    DocumentGroupId = table.Column<int>(nullable: false),
                    TeamWorkUserId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUserDocumentGroups", x => new { x.TeamWorkUserId, x.DocumentGroupId });
                    table.ForeignKey(
                        name: "FK_TeamWorkUserDocumentGroups_DocumentGroups_DocumentGroupId",
                        column: x => x.DocumentGroupId,
                        principalTable: "DocumentGroups",
                        principalColumn: "DocumentGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserDocumentGroups_TeamWorkUsers_TeamWorkUserId",
                        column: x => x.TeamWorkUserId,
                        principalTable: "TeamWorkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorkUserProductGroups",
                columns: table => new
                {
                    ProductGroupId = table.Column<int>(nullable: false),
                    TeamWorkUserId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUserProductGroups", x => new { x.TeamWorkUserId, x.ProductGroupId });
                    table.ForeignKey(
                        name: "FK_TeamWorkUserProductGroups_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserProductGroups_TeamWorkUsers_TeamWorkUserId",
                        column: x => x.TeamWorkUserId,
                        principalTable: "TeamWorkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorkUserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(nullable: false),
                    TeamWorkUserId = table.Column<int>(nullable: false),
                    TeamWorkId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ContractCode = table.Column<string>(type: "varchar(60)", nullable: true),
                    RoleName = table.Column<string>(nullable: true),
                    RoleDisplayName = table.Column<string>(nullable: true),
                    IsParallel = table.Column<bool>(nullable: false),
                    WorkFlowStateId = table.Column<int>(nullable: true),
                    SCMWorkFlow = table.Column<int>(nullable: false),
                    SCMModule = table.Column<int>(nullable: false),
                    SCMFormPermission = table.Column<int>(nullable: false),
                    SubModuleName = table.Column<string>(maxLength: 250, nullable: false),
                    SCMEvents = table.Column<string>(nullable: true),
                    IsSendNotification = table.Column<bool>(nullable: false),
                    IsSendMailNotification = table.Column<bool>(nullable: false),
                    IsSendSmsNotification = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserRoles_TeamWorkUsers_TeamWorkUserId",
                        column: x => x.TeamWorkUserId,
                        principalTable: "TeamWorkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamWorkUserWarehouses",
                columns: table => new
                {
                    TeamWorkUserId = table.Column<int>(nullable: false),
                    WarehouseId = table.Column<int>(nullable: false),
                    Id = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamWorkUserWarehouses", x => new { x.TeamWorkUserId, x.WarehouseId });
                    table.ForeignKey(
                        name: "FK_TeamWorkUserWarehouses_TeamWorkUsers_TeamWorkUserId",
                        column: x => x.TeamWorkUserId,
                        principalTable: "TeamWorkUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamWorkUserWarehouses_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationWorkFlows",
                columns: table => new
                {
                    ConfirmationWorkFlowId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentRevisionId = table.Column<long>(nullable: true),
                    ConfirmNote = table.Column<string>(maxLength: 800, nullable: true),
                    RevisionPageNumber = table.Column<int>(nullable: true),
                    RevisionPageSize = table.Column<string>(maxLength: 10, nullable: true),
                    Status = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationWorkFlows", x => x.ConfirmationWorkFlowId);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlows_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlows_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlows_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCommunications",
                columns: table => new
                {
                    DocumentCommunicationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    DocumentRevisionId = table.Column<long>(nullable: false),
                    CommunicationCode = table.Column<string>(maxLength: 64, nullable: false),
                    CommunicationType = table.Column<int>(nullable: false),
                    CommunicationStatus = table.Column<int>(nullable: false),
                    CommentStatus = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCommunications", x => x.DocumentCommunicationId);
                    table.ForeignKey(
                        name: "FK_DocumentCommunications_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentCommunications_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentCommunications_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentCommunications_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTQNCRs",
                columns: table => new
                {
                    DocumentTQNCRId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    DocumentRevisionId = table.Column<long>(nullable: false),
                    CommunicationCode = table.Column<string>(maxLength: 64, nullable: false),
                    Subject = table.Column<string>(maxLength: 250, nullable: true),
                    CommunicationType = table.Column<int>(nullable: false),
                    CommunicationStatus = table.Column<int>(nullable: false),
                    NCRReason = table.Column<int>(nullable: false),
                    CompanyIssue = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTQNCRs", x => x.DocumentTQNCRId);
                    table.ForeignKey(
                        name: "FK_DocumentTQNCRs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentTQNCRs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentTQNCRs_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentTQNCRs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentTQNCRs_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevisionActivities",
                columns: table => new
                {
                    RevisionActivityId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevisionId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    RevisionActivityStatus = table.Column<int>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    ActivityOwnerId = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Duration = table.Column<double>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionActivities", x => x.RevisionActivityId);
                    table.ForeignKey(
                        name: "FK_RevisionActivities_Users_ActivityOwnerId",
                        column: x => x.ActivityOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionActivities_DocumentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionComments",
                columns: table => new
                {
                    RevisionCommentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentRevisionId = table.Column<long>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ParentCommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionComments", x => x.RevisionCommentId);
                    table.ForeignKey(
                        name: "FK_RevisionComments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionComments_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionComments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionComments_RevisionComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "RevisionComments",
                        principalColumn: "RevisionCommentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransmittalRevisions",
                columns: table => new
                {
                    TransmittalId = table.Column<long>(nullable: false),
                    DocumentRevisionId = table.Column<long>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmittalRevisions", x => new { x.DocumentRevisionId, x.TransmittalId });
                    table.ForeignKey(
                        name: "FK_TransmittalRevisions_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransmittalRevisions_Transmittals_TransmittalId",
                        column: x => x.TransmittalId,
                        principalTable: "Transmittals",
                        principalColumn: "TransmittalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RFPItems",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPId = table.Column<long>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    PurchaseRequestItemId = table.Column<long>(nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPItems_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPItems_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFPItems_PurchaseRequestItems_PurchaseRequestItemId",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPItems_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    InvoiceNumber = table.Column<string>(maxLength: 64, nullable: false),
                    SupplierId = table.Column<int>(nullable: false),
                    POId = table.Column<long>(nullable: true),
                    InvoiceType = table.Column<int>(nullable: false),
                    PContractType = table.Column<int>(nullable: false),
                    CurrencyType = table.Column<int>(nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalProductAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    OtherCosts = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalInvoiceAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    InvoiceStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Packs",
                columns: table => new
                {
                    PackId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    POId = table.Column<long>(nullable: false),
                    PackCode = table.Column<string>(maxLength: 64, nullable: true),
                    PackStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packs", x => x.PackId);
                    table.ForeignKey(
                        name: "FK_Packs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packs_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POPreparations",
                columns: table => new
                {
                    POPreparationId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    POId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    DateStart = table.Column<DateTime>(nullable: true),
                    DateFinished = table.Column<DateTime>(nullable: true),
                    SubmitProgress = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    HasQualityControl = table.Column<bool>(nullable: false),
                    IsQualityControlDone = table.Column<bool>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POPreparations", x => x.POPreparationId);
                    table.ForeignKey(
                        name: "FK_POPreparations_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POPreparations_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POPreparations_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POService",
                columns: table => new
                {
                    POId = table.Column<long>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POService", x => new { x.POId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_POService_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POService_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POStatusLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    BeforeStatus = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    IsDone = table.Column<bool>(nullable: false),
                    POId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POStatusLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_POStatusLogs_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POStatusLogs_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POStatusLogs_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "POSubjects",
                columns: table => new
                {
                    POSubjectId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POId = table.Column<long>(nullable: true),
                    MrpItemId = table.Column<long>(nullable: true),
                    ParentSubjectId = table.Column<long>(nullable: true),
                    ProductId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    CoefficientUse = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ReceiptedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    POSubjectPartInvoiceStatus = table.Column<int>(nullable: false),
                    PriceUnit = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSubjects", x => x.POSubjectId);
                    table.ForeignKey(
                        name: "FK_POSubjects_MrpItems_MrpItemId",
                        column: x => x.MrpItemId,
                        principalTable: "MrpItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POSubjects_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POSubjects_POSubjects_ParentSubjectId",
                        column: x => x.ParentSubjectId,
                        principalTable: "POSubjects",
                        principalColumn: "POSubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoTermsOfPayments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PRContractId = table.Column<long>(nullable: true),
                    POId = table.Column<long>(nullable: true),
                    PaymentStep = table.Column<int>(nullable: false),
                    PaymentStatus = table.Column<int>(nullable: false),
                    IsCreditPayment = table.Column<bool>(nullable: false),
                    PaymentPercentage = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    CreditDuration = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoTermsOfPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoTermsOfPayments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoTermsOfPayments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoTermsOfPayments_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoTermsOfPayments_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PRContractSubjectPartLists",
                columns: table => new
                {
                    PRContractSubjectId = table.Column<long>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    Id = table.Column<long>(nullable: false),
                    CoefficientUse = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRContractSubjectPartLists", x => new { x.PRContractSubjectId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_PRContractSubjectPartLists_PRContractSubjects_PRContractSubjectId",
                        column: x => x.PRContractSubjectId,
                        principalTable: "PRContractSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PRContractSubjectPartLists_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RFPCommentInqueries",
                columns: table => new
                {
                    RFPInqueryId = table.Column<long>(nullable: false),
                    RFPCommentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPCommentInqueries", x => new { x.RFPCommentId, x.RFPInqueryId });
                    table.ForeignKey(
                        name: "FK_RFPCommentInqueries_RFPComments_RFPInqueryId",
                        column: x => x.RFPInqueryId,
                        principalTable: "RFPComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFPCommentInqueries_RFPInqueries_RFPInqueryId",
                        column: x => x.RFPInqueryId,
                        principalTable: "RFPInqueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPCommentUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RFPCommentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPCommentUsers", x => new { x.UserId, x.RFPCommentId });
                    table.ForeignKey(
                        name: "FK_RFPCommentUsers_RFPComments_RFPCommentId",
                        column: x => x.RFPCommentId,
                        principalTable: "RFPComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RFPCommentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RFPAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RFPId = table.Column<long>(nullable: true),
                    RFPInqueryId = table.Column<long>(nullable: true),
                    RFPSupplierProposalId = table.Column<long>(nullable: true),
                    RFPCommentId = table.Column<long>(nullable: true),
                    FileName = table.Column<string>(maxLength: 250, nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFPAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_RFPComments_RFPCommentId",
                        column: x => x.RFPCommentId,
                        principalTable: "RFPComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_RFPs_RFPId",
                        column: x => x.RFPId,
                        principalTable: "RFPs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_RFPInqueries_RFPInqueryId",
                        column: x => x.RFPInqueryId,
                        principalTable: "RFPInqueries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RFPAttachments_RFPSupplierProposals_RFPSupplierProposalId",
                        column: x => x.RFPSupplierProposalId,
                        principalTable: "RFPSupplierProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationWorkFlowUsers",
                columns: table => new
                {
                    ConfirmationWorkFlowUserId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfirmationWorkFlowId = table.Column<long>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    OrderNumber = table.Column<int>(nullable: false),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    IsBallInCourt = table.Column<bool>(nullable: false),
                    IsAccept = table.Column<bool>(nullable: false),
                    DateStart = table.Column<DateTime>(nullable: true),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationWorkFlowUsers", x => x.ConfirmationWorkFlowUserId);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowUsers_ConfirmationWorkFlows_ConfirmationWorkFlowId",
                        column: x => x.ConfirmationWorkFlowId,
                        principalTable: "ConfirmationWorkFlows",
                        principalColumn: "ConfirmationWorkFlowId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfirmationWorkFlowUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationQuestions",
                columns: table => new
                {
                    CommunicationQuestionId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    ParentQuestionId = table.Column<long>(nullable: true),
                    DocumentCommunicationId = table.Column<long>(nullable: true),
                    DocumentTQNCRId = table.Column<long>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IsReplyed = table.Column<bool>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationQuestions", x => x.CommunicationQuestionId);
                    table.ForeignKey(
                        name: "FK_CommunicationQuestions_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationQuestions_DocumentCommunications_DocumentCommunicationId",
                        column: x => x.DocumentCommunicationId,
                        principalTable: "DocumentCommunications",
                        principalColumn: "DocumentCommunicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationQuestions_DocumentTQNCRs_DocumentTQNCRId",
                        column: x => x.DocumentTQNCRId,
                        principalTable: "DocumentTQNCRs",
                        principalColumn: "DocumentTQNCRId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationQuestions_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationQuestions_CommunicationQuestions_ParentQuestionId",
                        column: x => x.ParentQuestionId,
                        principalTable: "CommunicationQuestions",
                        principalColumn: "CommunicationQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTeamComments",
                columns: table => new
                {
                    CommunicationTeamCommentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentCommunicationId = table.Column<long>(nullable: true),
                    DocumentTQNCRId = table.Column<long>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    ParentCommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTeamComments", x => x.CommunicationTeamCommentId);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamComments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamComments_DocumentCommunications_DocumentCommunicationId",
                        column: x => x.DocumentCommunicationId,
                        principalTable: "DocumentCommunications",
                        principalColumn: "DocumentCommunicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamComments_DocumentTQNCRs_DocumentTQNCRId",
                        column: x => x.DocumentTQNCRId,
                        principalTable: "DocumentTQNCRs",
                        principalColumn: "DocumentTQNCRId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamComments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamComments_CommunicationTeamComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "CommunicationTeamComments",
                        principalColumn: "CommunicationTeamCommentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevisionActivityTimesheets",
                columns: table => new
                {
                    ActivityTimesheetId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RevisionActivityId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(maxLength: 200, nullable: true),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    DateIssue = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionActivityTimesheets", x => x.ActivityTimesheetId);
                    table.ForeignKey(
                        name: "FK_RevisionActivityTimesheets_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionActivityTimesheets_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionActivityTimesheets_RevisionActivities_RevisionActivityId",
                        column: x => x.RevisionActivityId,
                        principalTable: "RevisionActivities",
                        principalColumn: "RevisionActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionCommentUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RevisionCommentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionCommentUsers", x => new { x.UserId, x.RevisionCommentId });
                    table.ForeignKey(
                        name: "FK_RevisionCommentUsers_RevisionComments_RevisionCommentId",
                        column: x => x.RevisionCommentId,
                        principalTable: "RevisionComments",
                        principalColumn: "RevisionCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevisionCommentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(nullable: false),
                    POId = table.Column<long>(nullable: true),
                    InvoiceId = table.Column<long>(nullable: true),
                    PaymentId = table.Column<long>(nullable: true),
                    CurrencyType = table.Column<int>(nullable: false),
                    FinancialAccountType = table.Column<int>(nullable: false),
                    PurchaseAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RejectPurchaseAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    InitialAccount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    DateDone = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceProducts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    TotalProductAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceProducts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(maxLength: 300, nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    PaymentId = table.Column<long>(nullable: true),
                    InvoiceId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAttachments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentAttachments_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Logistics",
                columns: table => new
                {
                    LogisticId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    PackId = table.Column<long>(nullable: false),
                    Step = table.Column<int>(nullable: false),
                    DateStart = table.Column<DateTime>(nullable: true),
                    DateEnd = table.Column<DateTime>(nullable: true),
                    LogisticStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logistics", x => x.LogisticId);
                    table.ForeignKey(
                        name: "FK_Logistics_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Logistics_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Logistics_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackingSubjects",
                columns: table => new
                {
                    PackSubjectId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackId = table.Column<long>(nullable: true),
                    ParentSubjectId = table.Column<long>(nullable: true),
                    ProductId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RemainedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingSubjects", x => x.PackSubjectId);
                    table.ForeignKey(
                        name: "FK_PackingSubjects_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackingSubjects_PackingSubjects_ParentSubjectId",
                        column: x => x.ParentSubjectId,
                        principalTable: "PackingSubjects",
                        principalColumn: "PackSubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackingSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    ReceiptId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ReceiptCode = table.Column<string>(maxLength: 64, nullable: false),
                    POId = table.Column<long>(nullable: false),
                    InvoiceId = table.Column<long>(nullable: true),
                    PackId = table.Column<long>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    ReceiptStatus = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.ReceiptId);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Receipts_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingForPayments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PendingForPaymentNumber = table.Column<string>(maxLength: 64, nullable: false),
                    PRContractId = table.Column<long>(nullable: true),
                    BaseContractCode = table.Column<string>(maxLength: 60, nullable: true),
                    POId = table.Column<long>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    POTermsOfPaymentId = table.Column<long>(nullable: true),
                    InvoiceId = table.Column<long>(nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    AmountPayed = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    AmountRemained = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Status = table.Column<int>(nullable: false),
                    PaymentDateTime = table.Column<DateTime>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingForPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_Contracts_BaseContractCode",
                        column: x => x.BaseContractCode,
                        principalTable: "Contracts",
                        principalColumn: "ContractCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_PoTermsOfPayments_POTermsOfPaymentId",
                        column: x => x.POTermsOfPaymentId,
                        principalTable: "PoTermsOfPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PendingForPayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationReplys",
                columns: table => new
                {
                    CommunicationReplyId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    CommunicationQuestionId = table.Column<long>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationReplys", x => x.CommunicationReplyId);
                    table.ForeignKey(
                        name: "FK_CommunicationReplys_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationReplys_CommunicationQuestions_CommunicationQuestionId",
                        column: x => x.CommunicationQuestionId,
                        principalTable: "CommunicationQuestions",
                        principalColumn: "CommunicationQuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationReplys_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTeamCommentUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    CommunicationTeamCommentId = table.Column<long>(nullable: false),
                    Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTeamCommentUsers", x => new { x.UserId, x.CommunicationTeamCommentId });
                    table.ForeignKey(
                        name: "FK_CommunicationTeamCommentUsers_CommunicationTeamComments_CommunicationTeamCommentId",
                        column: x => x.CommunicationTeamCommentId,
                        principalTable: "CommunicationTeamComments",
                        principalColumn: "CommunicationTeamCommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunicationTeamCommentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionAttachments",
                columns: table => new
                {
                    RevisionAttachmentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentRevisionId = table.Column<long>(nullable: true),
                    RevisionActivityTimesheetId = table.Column<long>(nullable: true),
                    RevisionCommentId = table.Column<long>(nullable: true),
                    ConfirmationWorkFlowId = table.Column<long>(nullable: true),
                    TransmittalId = table.Column<long>(nullable: true),
                    RevisionAttachmentType = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(maxLength: 250, nullable: true),
                    FileSrc = table.Column<string>(maxLength: 250, nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionAttachments", x => x.RevisionAttachmentId);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_ConfirmationWorkFlows_ConfirmationWorkFlowId",
                        column: x => x.ConfirmationWorkFlowId,
                        principalTable: "ConfirmationWorkFlows",
                        principalColumn: "ConfirmationWorkFlowId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_DocumentRevisions_DocumentRevisionId",
                        column: x => x.DocumentRevisionId,
                        principalTable: "DocumentRevisions",
                        principalColumn: "DocumentRevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_RevisionActivityTimesheets_RevisionActivityTimesheetId",
                        column: x => x.RevisionActivityTimesheetId,
                        principalTable: "RevisionActivityTimesheets",
                        principalColumn: "ActivityTimesheetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_RevisionComments_RevisionCommentId",
                        column: x => x.RevisionCommentId,
                        principalTable: "RevisionComments",
                        principalColumn: "RevisionCommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionAttachments_Transmittals_TransmittalId",
                        column: x => x.TransmittalId,
                        principalTable: "Transmittals",
                        principalColumn: "TransmittalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityControls",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    POPreparationId = table.Column<long>(nullable: true),
                    ReceiptId = table.Column<long>(nullable: true),
                    PackId = table.Column<long>(nullable: true),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    QCResult = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityControls_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityControls_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityControls_POPreparations_POPreparationId",
                        column: x => x.POPreparationId,
                        principalTable: "POPreparations",
                        principalColumn: "POPreparationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityControls_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityControls_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptRejects",
                columns: table => new
                {
                    ReceiptRejectId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ReceiptRejectCode = table.Column<string>(maxLength: 64, nullable: false),
                    POId = table.Column<long>(nullable: false),
                    InvoiceId = table.Column<long>(nullable: true),
                    SupplierId = table.Column<int>(nullable: true),
                    PackId = table.Column<long>(nullable: true),
                    ReceiptId = table.Column<long>(nullable: true),
                    Note = table.Column<string>(maxLength: 800, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptRejects", x => x.ReceiptRejectId);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejects_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptSubjects",
                columns: table => new
                {
                    ReceiptSubjectId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(nullable: false),
                    ReceiptId = table.Column<long>(nullable: true),
                    ParentSubjectId = table.Column<long>(nullable: true),
                    PackQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    ReceiptQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    QCAcceptQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    PurchaseRejectRemainedQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptSubjects", x => x.ReceiptSubjectId);
                    table.ForeignKey(
                        name: "FK_ReceiptSubjects_ReceiptSubjects_ParentSubjectId",
                        column: x => x.ParentSubjectId,
                        principalTable: "ReceiptSubjects",
                        principalColumn: "ReceiptSubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptSubjects_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentPendingForPayments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<long>(nullable: false),
                    PendingForPaymentId = table.Column<long>(nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPendingForPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentPendingForPayments_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentPendingForPayments_PendingForPayments_PendingForPaymentId",
                        column: x => x.PendingForPaymentId,
                        principalTable: "PendingForPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationAttachments",
                columns: table => new
                {
                    CommunicationAttachmentId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DocumentCommunicationId = table.Column<long>(nullable: true),
                    DocumentTQNCRId = table.Column<long>(nullable: true),
                    CommunicationTeamCommentId = table.Column<long>(nullable: true),
                    CommunicationQuestionId = table.Column<long>(nullable: true),
                    CommunicationReplyId = table.Column<long>(nullable: true),
                    FileName = table.Column<string>(maxLength: 250, nullable: true),
                    FileSrc = table.Column<string>(maxLength: 250, nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationAttachments", x => x.CommunicationAttachmentId);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_CommunicationQuestions_CommunicationQuestionId",
                        column: x => x.CommunicationQuestionId,
                        principalTable: "CommunicationQuestions",
                        principalColumn: "CommunicationQuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_CommunicationReplys_CommunicationReplyId",
                        column: x => x.CommunicationReplyId,
                        principalTable: "CommunicationReplys",
                        principalColumn: "CommunicationReplyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_CommunicationTeamComments_CommunicationTeamCommentId",
                        column: x => x.CommunicationTeamCommentId,
                        principalTable: "CommunicationTeamComments",
                        principalColumn: "CommunicationTeamCommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_DocumentCommunications_DocumentCommunicationId",
                        column: x => x.DocumentCommunicationId,
                        principalTable: "DocumentCommunications",
                        principalColumn: "DocumentCommunicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_DocumentTQNCRs_DocumentTQNCRId",
                        column: x => x.DocumentTQNCRId,
                        principalTable: "DocumentTQNCRs",
                        principalColumn: "DocumentTQNCRId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunicationAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    AdderUserId = table.Column<int>(nullable: true),
                    ModifierUserId = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PRContractId = table.Column<long>(nullable: true),
                    PurchaseRequestId = table.Column<long>(nullable: true),
                    POId = table.Column<long>(nullable: true),
                    POPreparationId = table.Column<long>(nullable: true),
                    QualityControlId = table.Column<long>(nullable: true),
                    PackId = table.Column<long>(nullable: true),
                    LogisticId = table.Column<long>(nullable: true),
                    ReceiptId = table.Column<long>(nullable: true),
                    ReceiptRejectId = table.Column<long>(nullable: true),
                    FileName = table.Column<string>(maxLength: 250, nullable: true),
                    FileSize = table.Column<long>(nullable: false),
                    FileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAttachments_Users_AdderUserId",
                        column: x => x.AdderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_Logistics_LogisticId",
                        column: x => x.LogisticId,
                        principalTable: "Logistics",
                        principalColumn: "LogisticId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_Users_ModifierUserId",
                        column: x => x.ModifierUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_POs_POId",
                        column: x => x.POId,
                        principalTable: "POs",
                        principalColumn: "POId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_POPreparations_POPreparationId",
                        column: x => x.POPreparationId,
                        principalTable: "POPreparations",
                        principalColumn: "POPreparationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_PRContracts_PRContractId",
                        column: x => x.PRContractId,
                        principalTable: "PRContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_Packs_PackId",
                        column: x => x.PackId,
                        principalTable: "Packs",
                        principalColumn: "PackId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_QualityControls_QualityControlId",
                        column: x => x.QualityControlId,
                        principalTable: "QualityControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAttachments_ReceiptRejects_ReceiptRejectId",
                        column: x => x.ReceiptRejectId,
                        principalTable: "ReceiptRejects",
                        principalColumn: "ReceiptRejectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptRejectSubjects",
                columns: table => new
                {
                    ReceiptRejectSubjectId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(nullable: false),
                    ReceiptRejectId = table.Column<long>(nullable: true),
                    ParentSubjectId = table.Column<long>(nullable: true),
                    ReceiptQuantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptRejectSubjects", x => x.ReceiptRejectSubjectId);
                    table.ForeignKey(
                        name: "FK_ReceiptRejectSubjects_ReceiptRejectSubjects_ParentSubjectId",
                        column: x => x.ParentSubjectId,
                        principalTable: "ReceiptRejectSubjects",
                        principalColumn: "ReceiptRejectSubjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptRejectSubjects_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptRejectSubjects_ReceiptRejects_ReceiptRejectId",
                        column: x => x.ReceiptRejectId,
                        principalTable: "ReceiptRejects",
                        principalColumn: "ReceiptRejectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseProductStockLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(nullable: false),
                    DateChange = table.Column<DateTime>(nullable: false),
                    ReceiptId = table.Column<long>(nullable: true),
                    WarehouseTransferenceId = table.Column<long>(nullable: true),
                    WarehouseStockChangeActionType = table.Column<int>(nullable: false),
                    Input = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Output = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    RealStock = table.Column<decimal>(type: "decimal(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseProductStockLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseProductStockLogs_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseProductStockLogs_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "ReceiptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseProductStockLogs_ReceiptRejects_WarehouseTransferenceId",
                        column: x => x.WarehouseTransferenceId,
                        principalTable: "ReceiptRejects",
                        principalColumn: "ReceiptRejectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AdderUserId",
                table: "Addresses",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CompanyId",
                table: "Addresses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ModifierUserId",
                table: "Addresses",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BomProducts_AdderUserId",
                table: "BomProducts",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BomProducts_ModifierUserId",
                table: "BomProducts",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BomProducts_ParentBomId",
                table: "BomProducts",
                column: "ParentBomId");

            migrationBuilder.CreateIndex(
                name: "IX_BomProducts_ProductId",
                table: "BomProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_AdderUserId",
                table: "Budgetings",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_BudgetCode",
                table: "Budgetings",
                column: "BudgetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_ModifierUserId",
                table: "Budgetings",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgetings_MrpId",
                table: "Budgetings",
                column: "MrpId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_AdderUserId",
                table: "BudgetingSubjects",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_BudgetingId",
                table: "BudgetingSubjects",
                column: "BudgetingId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_ModifierUserId",
                table: "BudgetingSubjects",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetingSubjects_ProductId",
                table: "BudgetingSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_ProvinceId",
                table: "Cities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_AdderUserId",
                table: "CommunicationAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_CommunicationQuestionId",
                table: "CommunicationAttachments",
                column: "CommunicationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_CommunicationReplyId",
                table: "CommunicationAttachments",
                column: "CommunicationReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_CommunicationTeamCommentId",
                table: "CommunicationAttachments",
                column: "CommunicationTeamCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_DocumentCommunicationId",
                table: "CommunicationAttachments",
                column: "DocumentCommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_DocumentTQNCRId",
                table: "CommunicationAttachments",
                column: "DocumentTQNCRId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationAttachments_ModifierUserId",
                table: "CommunicationAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationQuestions_AdderUserId",
                table: "CommunicationQuestions",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationQuestions_DocumentCommunicationId",
                table: "CommunicationQuestions",
                column: "DocumentCommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationQuestions_DocumentTQNCRId",
                table: "CommunicationQuestions",
                column: "DocumentTQNCRId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationQuestions_ModifierUserId",
                table: "CommunicationQuestions",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationQuestions_ParentQuestionId",
                table: "CommunicationQuestions",
                column: "ParentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationReplys_AdderUserId",
                table: "CommunicationReplys",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationReplys_CommunicationQuestionId",
                table: "CommunicationReplys",
                column: "CommunicationQuestionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationReplys_ModifierUserId",
                table: "CommunicationReplys",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamComments_AdderUserId",
                table: "CommunicationTeamComments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamComments_DocumentCommunicationId",
                table: "CommunicationTeamComments",
                column: "DocumentCommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamComments_DocumentTQNCRId",
                table: "CommunicationTeamComments",
                column: "DocumentTQNCRId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamComments_ModifierUserId",
                table: "CommunicationTeamComments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamComments_ParentCommentId",
                table: "CommunicationTeamComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationTeamCommentUsers_CommunicationTeamCommentId",
                table: "CommunicationTeamCommentUsers",
                column: "CommunicationTeamCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_AdderUserId",
                table: "CompanyUsers",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CustomerId",
                table: "CompanyUsers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_ModifierUserId",
                table: "CompanyUsers",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_SupplierId",
                table: "CompanyUsers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlows_AdderUserId",
                table: "ConfirmationWorkFlows",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlows_DocumentRevisionId",
                table: "ConfirmationWorkFlows",
                column: "DocumentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlows_ModifierUserId",
                table: "ConfirmationWorkFlows",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowTemplates_AdderUserId",
                table: "ConfirmationWorkFlowTemplates",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowTemplates_DocumentGroupId",
                table: "ConfirmationWorkFlowTemplates",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowTemplates_ModifierUserId",
                table: "ConfirmationWorkFlowTemplates",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowTemplateUsers_ConfirmationWorkFlowTemplateId",
                table: "ConfirmationWorkFlowTemplateUsers",
                column: "ConfirmationWorkFlowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowTemplateUsers_UserId",
                table: "ConfirmationWorkFlowTemplateUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowUsers_ConfirmationWorkFlowId",
                table: "ConfirmationWorkFlowUsers",
                column: "ConfirmationWorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationWorkFlowUsers_UserId",
                table: "ConfirmationWorkFlowUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAddresses_ContractCode",
                table: "ContractAddresses",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_AdderUserId",
                table: "ContractAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_ContractCode",
                table: "ContractAttachments",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_ModifierUserId",
                table: "ContractAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentGroupLists_AdderUserId",
                table: "ContractDocumentGroupLists",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentGroupLists_ContractCode",
                table: "ContractDocumentGroupLists",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentGroupLists_DocumentGroupId",
                table: "ContractDocumentGroupLists",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocumentGroupLists_ModifierUserId",
                table: "ContractDocumentGroupLists",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_AdderUserId",
                table: "Contracts",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CustomerId",
                table: "Contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ModifierUserId",
                table: "Contracts",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ParnetContractCode",
                table: "Contracts",
                column: "ParnetContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSubjects_AdderUserId",
                table: "ContractSubjects",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSubjects_ContractCode",
                table: "ContractSubjects",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSubjects_ModifierUserId",
                table: "ContractSubjects",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSubjects_ProductId",
                table: "ContractSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AdderUserId",
                table: "Customers",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ModifierUserId",
                table: "Customers",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_AdderUserId",
                table: "DocumentCommunications",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_CommunicationCode",
                table: "DocumentCommunications",
                column: "CommunicationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_CustomerId",
                table: "DocumentCommunications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_DocumentRevisionId",
                table: "DocumentCommunications",
                column: "DocumentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCommunications_ModifierUserId",
                table: "DocumentCommunications",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentGroups_AdderUserId",
                table: "DocumentGroups",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentGroups_ModifierUserId",
                table: "DocumentGroups",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentProducts_DocumentId",
                table: "DocumentProducts",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRevisions_AdderUserId",
                table: "DocumentRevisions",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRevisions_DocumentId",
                table: "DocumentRevisions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRevisions_ModifierUserId",
                table: "DocumentRevisions",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AdderUserId",
                table: "Documents",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ConfirmationWorkFlowTemplateId",
                table: "Documents",
                column: "ConfirmationWorkFlowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ContractCode",
                table: "Documents",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocNumber",
                table: "Documents",
                column: "DocNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentGroupId",
                table: "Documents",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ModifierUserId",
                table: "Documents",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_AdderUserId",
                table: "DocumentTQNCRs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_CommunicationCode",
                table: "DocumentTQNCRs",
                column: "CommunicationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_CustomerId",
                table: "DocumentTQNCRs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_DocumentRevisionId",
                table: "DocumentTQNCRs",
                column: "DocumentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_ModifierUserId",
                table: "DocumentTQNCRs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTQNCRs_SupplierId",
                table: "DocumentTQNCRs",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_InvoiceId",
                table: "FinancialAccounts",
                column: "InvoiceId",
                unique: true,
                filter: "[InvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_POId",
                table: "FinancialAccounts",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_PaymentId",
                table: "FinancialAccounts",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_SupplierId",
                table: "FinancialAccounts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProducts_InvoiceId",
                table: "InvoiceProducts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProducts_ProductId",
                table: "InvoiceProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AdderUserId",
                table: "Invoices",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ModifierUserId",
                table: "Invoices",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_POId",
                table: "Invoices",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SupplierId",
                table: "Invoices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Logistics_AdderUserId",
                table: "Logistics",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Logistics_ModifierUserId",
                table: "Logistics",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Logistics_PackId",
                table: "Logistics",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_LogUserReceivers_SCMAuditLogId",
                table: "LogUserReceivers",
                column: "SCMAuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterMRs_BomProductId",
                table: "MasterMRs",
                column: "BomProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterMRs_ContractCode",
                table: "MasterMRs",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_MasterMRs_ProductId",
                table: "MasterMRs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpItems_AdderUserId",
                table: "MrpItems",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpItems_ModifierUserId",
                table: "MrpItems",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpItems_MrpId",
                table: "MrpItems",
                column: "MrpId");

            migrationBuilder.CreateIndex(
                name: "IX_MrpItems_ProductId",
                table: "MrpItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Mrps_AdderUserId",
                table: "Mrps",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mrps_ContractCode",
                table: "Mrps",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Mrps_ModifierUserId",
                table: "Mrps",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mrps_MrpNumber",
                table: "Mrps",
                column: "MrpNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PerformerUserId",
                table: "Notifications",
                column: "PerformerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingSubjects_PackId",
                table: "PackingSubjects",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingSubjects_ParentSubjectId",
                table: "PackingSubjects",
                column: "ParentSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingSubjects_ProductId",
                table: "PackingSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_AdderUserId",
                table: "Packs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_ModifierUserId",
                table: "Packs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_POId",
                table: "Packs",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_Packs_PackCode",
                table: "Packs",
                column: "PackCode",
                unique: true,
                filter: "[PackCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_AdderUserId",
                table: "PAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_LogisticId",
                table: "PAttachments",
                column: "LogisticId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_ModifierUserId",
                table: "PAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_POId",
                table: "PAttachments",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_POPreparationId",
                table: "PAttachments",
                column: "POPreparationId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_PRContractId",
                table: "PAttachments",
                column: "PRContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_PackId",
                table: "PAttachments",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_PurchaseRequestId",
                table: "PAttachments",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_QualityControlId",
                table: "PAttachments",
                column: "QualityControlId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_ReceiptId",
                table: "PAttachments",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_PAttachments_ReceiptRejectId",
                table: "PAttachments",
                column: "ReceiptRejectId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttachments_AdderUserId",
                table: "PaymentAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttachments_InvoiceId",
                table: "PaymentAttachments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttachments_ModifierUserId",
                table: "PaymentAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttachments_PaymentId",
                table: "PaymentAttachments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPendingForPayments_PaymentId",
                table: "PaymentPendingForPayments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPendingForPayments_PendingForPaymentId",
                table: "PaymentPendingForPayments",
                column: "PendingForPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AdderUserId",
                table: "Payments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractCode",
                table: "Payments",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ModifierUserId",
                table: "Payments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNumber",
                table: "Payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SupplierId",
                table: "Payments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_AdderUserId",
                table: "PendingForPayments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_BaseContractCode",
                table: "PendingForPayments",
                column: "BaseContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_InvoiceId",
                table: "PendingForPayments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_ModifierUserId",
                table: "PendingForPayments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_POId",
                table: "PendingForPayments",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_POTermsOfPaymentId",
                table: "PendingForPayments",
                column: "POTermsOfPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_PRContractId",
                table: "PendingForPayments",
                column: "PRContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_PendingForPaymentNumber",
                table: "PendingForPayments",
                column: "PendingForPaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingForPayments_SupplierId",
                table: "PendingForPayments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_AdderUserId",
                table: "POPreparations",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_ModifierUserId",
                table: "POPreparations",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POPreparations_POId",
                table: "POPreparations",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_POs_AdderUserId",
                table: "POs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POs_BaseContractCode",
                table: "POs",
                column: "BaseContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_POs_ModifierUserId",
                table: "POs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POs_POCode",
                table: "POs",
                column: "POCode",
                unique: true,
                filter: "[POCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_POs_PRContractId",
                table: "POs",
                column: "PRContractId");

            migrationBuilder.CreateIndex(
                name: "IX_POs_ServiceId",
                table: "POs",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_POs_SupplierId",
                table: "POs",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_POService_ServiceId",
                table: "POService",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_POStatusLogs_AdderUserId",
                table: "POStatusLogs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POStatusLogs_ModifierUserId",
                table: "POStatusLogs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_POStatusLogs_POId",
                table: "POStatusLogs",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_POSubjects_MrpItemId",
                table: "POSubjects",
                column: "MrpItemId");

            migrationBuilder.CreateIndex(
                name: "IX_POSubjects_POId",
                table: "POSubjects",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_POSubjects_ParentSubjectId",
                table: "POSubjects",
                column: "ParentSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_POSubjects_ProductId",
                table: "POSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PoTermsOfPayments_AdderUserId",
                table: "PoTermsOfPayments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoTermsOfPayments_ModifierUserId",
                table: "PoTermsOfPayments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PoTermsOfPayments_POId",
                table: "PoTermsOfPayments",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_PoTermsOfPayments_PRContractId",
                table: "PoTermsOfPayments",
                column: "PRContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PRConfirmLogs_AdderUserId",
                table: "PRConfirmLogs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRConfirmLogs_ModifierUserId",
                table: "PRConfirmLogs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRConfirmLogs_PurchaseRequestId",
                table: "PRConfirmLogs",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_AdderUserId",
                table: "PRContracts",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_ModifierUserId",
                table: "PRContracts",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_PRContractCode",
                table: "PRContracts",
                column: "PRContractCode",
                unique: true,
                filter: "[PRContractCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_RFPId",
                table: "PRContracts",
                column: "RFPId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_ServiceId",
                table: "PRContracts",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContracts_SupplierId",
                table: "PRContracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractServices_ServiceId",
                table: "PRContractServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjectPartLists_ProductId",
                table: "PRContractSubjectPartLists",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjects_AdderUserId",
                table: "PRContractSubjects",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjects_ModifierUserId",
                table: "PRContractSubjects",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjects_PRContractId",
                table: "PRContractSubjects",
                column: "PRContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PRContractSubjects_ProductId",
                table: "PRContractSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_AdderUserId",
                table: "ProductGroups",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_ModifierUserId",
                table: "ProductGroups",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_ProductGroupCode",
                table: "ProductGroups",
                column: "ProductGroupCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_AdderUserId",
                table: "Products",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Description",
                table: "Products",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ModifierUserId",
                table: "Products",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductGroupId",
                table: "Products",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_AdderUserId",
                table: "PurchaseRequestItems",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_ModifierUserId",
                table: "PurchaseRequestItems",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_ProductId",
                table: "PurchaseRequestItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestItems_PurchaseRequestId",
                table: "PurchaseRequestItems",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_AdderUserId",
                table: "PurchaseRequests",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_ContractCode",
                table: "PurchaseRequests",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_ModifierUserId",
                table: "PurchaseRequests",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_MrpId",
                table: "PurchaseRequests",
                column: "MrpId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_PRCode",
                table: "PurchaseRequests",
                column: "PRCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_AdderUserId",
                table: "QualityControls",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_ModifierUserId",
                table: "QualityControls",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_POPreparationId",
                table: "QualityControls",
                column: "POPreparationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_PackId",
                table: "QualityControls",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityControls_ReceiptId",
                table: "QualityControls",
                column: "ReceiptId",
                unique: true,
                filter: "[ReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_AdderUserId",
                table: "ReceiptRejects",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_InvoiceId",
                table: "ReceiptRejects",
                column: "InvoiceId",
                unique: true,
                filter: "[InvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_ModifierUserId",
                table: "ReceiptRejects",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_POId",
                table: "ReceiptRejects",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_PackId",
                table: "ReceiptRejects",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_ReceiptId",
                table: "ReceiptRejects",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_ReceiptRejectCode",
                table: "ReceiptRejects",
                column: "ReceiptRejectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejects_SupplierId",
                table: "ReceiptRejects",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejectSubjects_ParentSubjectId",
                table: "ReceiptRejectSubjects",
                column: "ParentSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejectSubjects_ProductId",
                table: "ReceiptRejectSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRejectSubjects_ReceiptRejectId",
                table: "ReceiptRejectSubjects",
                column: "ReceiptRejectId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_AdderUserId",
                table: "Receipts",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_InvoiceId",
                table: "Receipts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ModifierUserId",
                table: "Receipts",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_POId",
                table: "Receipts",
                column: "POId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PackId",
                table: "Receipts",
                column: "PackId",
                unique: true,
                filter: "[PackId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptCode",
                table: "Receipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SupplierId",
                table: "Receipts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSubjects_ParentSubjectId",
                table: "ReceiptSubjects",
                column: "ParentSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSubjects_ProductId",
                table: "ReceiptSubjects",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptSubjects_ReceiptId",
                table: "ReceiptSubjects",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivities_ActivityOwnerId",
                table: "RevisionActivities",
                column: "ActivityOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivities_RevisionId",
                table: "RevisionActivities",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivityTimesheets_AdderUserId",
                table: "RevisionActivityTimesheets",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivityTimesheets_ModifierUserId",
                table: "RevisionActivityTimesheets",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionActivityTimesheets_RevisionActivityId",
                table: "RevisionActivityTimesheets",
                column: "RevisionActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_AdderUserId",
                table: "RevisionAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_ConfirmationWorkFlowId",
                table: "RevisionAttachments",
                column: "ConfirmationWorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_DocumentRevisionId",
                table: "RevisionAttachments",
                column: "DocumentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_ModifierUserId",
                table: "RevisionAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_RevisionActivityTimesheetId",
                table: "RevisionAttachments",
                column: "RevisionActivityTimesheetId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_RevisionCommentId",
                table: "RevisionAttachments",
                column: "RevisionCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionAttachments_TransmittalId",
                table: "RevisionAttachments",
                column: "TransmittalId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionComments_AdderUserId",
                table: "RevisionComments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionComments_DocumentRevisionId",
                table: "RevisionComments",
                column: "DocumentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionComments_ModifierUserId",
                table: "RevisionComments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionComments_ParentCommentId",
                table: "RevisionComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionCommentUsers_RevisionCommentId",
                table: "RevisionCommentUsers",
                column: "RevisionCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_AdderUserId",
                table: "RFPAttachments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_ModifierUserId",
                table: "RFPAttachments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_RFPCommentId",
                table: "RFPAttachments",
                column: "RFPCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_RFPId",
                table: "RFPAttachments",
                column: "RFPId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_RFPInqueryId",
                table: "RFPAttachments",
                column: "RFPInqueryId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPAttachments_RFPSupplierProposalId",
                table: "RFPAttachments",
                column: "RFPSupplierProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPCommentInqueries_RFPInqueryId",
                table: "RFPCommentInqueries",
                column: "RFPInqueryId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPComments_AdderUserId",
                table: "RFPComments",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPComments_ModifierUserId",
                table: "RFPComments",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPComments_ParentCommentId",
                table: "RFPComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPComments_RFPSupplierId",
                table: "RFPComments",
                column: "RFPSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPCommentUsers_RFPCommentId",
                table: "RFPCommentUsers",
                column: "RFPCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPInqueries_AdderUserId",
                table: "RFPInqueries",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPInqueries_ModifierUserId",
                table: "RFPInqueries",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPInqueries_RFPId",
                table: "RFPInqueries",
                column: "RFPId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPItems_AdderUserId",
                table: "RFPItems",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPItems_ModifierUserId",
                table: "RFPItems",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPItems_ProductId",
                table: "RFPItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPItems_PurchaseRequestItemId",
                table: "RFPItems",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPItems_RFPId",
                table: "RFPItems",
                column: "RFPId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPs_AdderUserId",
                table: "RFPs",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPs_ContractCode",
                table: "RFPs",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_RFPs_ModifierUserId",
                table: "RFPs",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPs_RFPNumber",
                table: "RFPs",
                column: "RFPNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RFPSupplierProposals_AdderUserId",
                table: "RFPSupplierProposals",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSupplierProposals_ModifierUserId",
                table: "RFPSupplierProposals",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSupplierProposals_RFPInqueryId",
                table: "RFPSupplierProposals",
                column: "RFPInqueryId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSupplierProposals_RFPSupplierId",
                table: "RFPSupplierProposals",
                column: "RFPSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSuppliers_AdderUserId",
                table: "RFPSuppliers",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSuppliers_ModifierUserId",
                table: "RFPSuppliers",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSuppliers_RFPId",
                table: "RFPSuppliers",
                column: "RFPId");

            migrationBuilder.CreateIndex(
                name: "IX_RFPSuppliers_SupplierId",
                table: "RFPSuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true)
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_SCMAuditLogs_BaseContractCode",
                table: "SCMAuditLogs",
                column: "BaseContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_SCMAuditLogs_DocumentGroupId",
                table: "SCMAuditLogs",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SCMAuditLogs_PerformerUserId",
                table: "SCMAuditLogs",
                column: "PerformerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SCMAuditLogs_ProductGroupId",
                table: "SCMAuditLogs",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_AdderUserId",
                table: "Services",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ModifierUserId",
                table: "Services",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceCode",
                table: "Services",
                column: "ServiceCode",
                unique: true,
                filter: "[ServiceCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProductGroups_ProductGroupId",
                table: "SupplierProductGroups",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_AdderUserId",
                table: "Suppliers",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_ModifierUserId",
                table: "Suppliers",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorks_ContractCode",
                table: "TeamWorks",
                column: "ContractCode",
                unique: true,
                filter: "[ContractCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserDocumentGroups_DocumentGroupId",
                table: "TeamWorkUserDocumentGroups",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserProductGroups_ProductGroupId",
                table: "TeamWorkUserProductGroups",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserRoles_RoleId",
                table: "TeamWorkUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserRoles_TeamWorkUserId",
                table: "TeamWorkUserRoles",
                column: "TeamWorkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUsers_TeamWorkId",
                table: "TeamWorkUsers",
                column: "TeamWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUsers_UserId",
                table: "TeamWorkUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWorkUserWarehouses_WarehouseId",
                table: "TeamWorkUserWarehouses",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmittalRevisions_TransmittalId",
                table: "TransmittalRevisions",
                column: "TransmittalId");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_AdderUserId",
                table: "Transmittals",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_ContractCode",
                table: "Transmittals",
                column: "ContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_DocumentGroupId",
                table: "Transmittals",
                column: "DocumentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_ModifierUserId",
                table: "Transmittals",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_SupplierId",
                table: "Transmittals",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Transmittals_TransmittalNumber",
                table: "Transmittals",
                column: "TransmittalNumber",
                unique: true,
                filter: "[TransmittalNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvisibleTeamWorks_UserId",
                table: "UserInvisibleTeamWorks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSeenSCMAuditLogs_SCMAuditLogId",
                table: "UserSeenSCMAuditLogs",
                column: "SCMAuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProducts_AdderUserId",
                table: "WarehouseProducts",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProducts_ModifierUserId",
                table: "WarehouseProducts",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProducts_ProductId",
                table: "WarehouseProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProductStockLogs_ProductId",
                table: "WarehouseProductStockLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProductStockLogs_ReceiptId",
                table: "WarehouseProductStockLogs",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseProductStockLogs_WarehouseTransferenceId",
                table: "WarehouseProductStockLogs",
                column: "WarehouseTransferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AdderUserId",
                table: "Warehouses",
                column: "AdderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_AddressId",
                table: "Warehouses",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_ModifierUserId",
                table: "Warehouses",
                column: "ModifierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseCode",
                table: "Warehouses",
                column: "WarehouseCode",
                unique: true,
                filter: "[WarehouseCode] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomProducts");

            migrationBuilder.DropTable(
                name: "BudgetingSubjects");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "CommunicationAttachments");

            migrationBuilder.DropTable(
                name: "CommunicationTeamCommentUsers");

            migrationBuilder.DropTable(
                name: "CompanyUsers");

            migrationBuilder.DropTable(
                name: "ConfirmationWorkFlowTemplateUsers");

            migrationBuilder.DropTable(
                name: "ConfirmationWorkFlowUsers");

            migrationBuilder.DropTable(
                name: "ContractAddresses");

            migrationBuilder.DropTable(
                name: "ContractAttachments");

            migrationBuilder.DropTable(
                name: "ContractDocumentGroupLists");

            migrationBuilder.DropTable(
                name: "ContractSubjects");

            migrationBuilder.DropTable(
                name: "DocumentProducts");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropTable(
                name: "InvoiceProducts");

            migrationBuilder.DropTable(
                name: "LogUserReceivers");

            migrationBuilder.DropTable(
                name: "MasterMRs");

            migrationBuilder.DropTable(
                name: "PackingSubjects");

            migrationBuilder.DropTable(
                name: "PAttachments");

            migrationBuilder.DropTable(
                name: "PaymentAttachments");

            migrationBuilder.DropTable(
                name: "PaymentPendingForPayments");

            migrationBuilder.DropTable(
                name: "POService");

            migrationBuilder.DropTable(
                name: "POStatusLogs");

            migrationBuilder.DropTable(
                name: "POSubjects");

            migrationBuilder.DropTable(
                name: "PRConfirmLogs");

            migrationBuilder.DropTable(
                name: "PRContractServices");

            migrationBuilder.DropTable(
                name: "PRContractSubjectPartLists");

            migrationBuilder.DropTable(
                name: "ProductUnits");

            migrationBuilder.DropTable(
                name: "ReceiptRejectSubjects");

            migrationBuilder.DropTable(
                name: "ReceiptSubjects");

            migrationBuilder.DropTable(
                name: "RevisionAttachments");

            migrationBuilder.DropTable(
                name: "RevisionCommentUsers");

            migrationBuilder.DropTable(
                name: "RFPAttachments");

            migrationBuilder.DropTable(
                name: "RFPCommentInqueries");

            migrationBuilder.DropTable(
                name: "RFPCommentUsers");

            migrationBuilder.DropTable(
                name: "RFPItems");

            migrationBuilder.DropTable(
                name: "SupplierProductGroups");

            migrationBuilder.DropTable(
                name: "TeamWorkUserDocumentGroups");

            migrationBuilder.DropTable(
                name: "TeamWorkUserProductGroups");

            migrationBuilder.DropTable(
                name: "TeamWorkUserRoles");

            migrationBuilder.DropTable(
                name: "TeamWorkUserWarehouses");

            migrationBuilder.DropTable(
                name: "TransmittalRevisions");

            migrationBuilder.DropTable(
                name: "UserInvisibleTeamWorks");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "UserSeenSCMAuditLogs");

            migrationBuilder.DropTable(
                name: "WarehouseProducts");

            migrationBuilder.DropTable(
                name: "WarehouseProductStockLogs");

            migrationBuilder.DropTable(
                name: "Budgetings");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "CommunicationReplys");

            migrationBuilder.DropTable(
                name: "CommunicationTeamComments");

            migrationBuilder.DropTable(
                name: "Logistics");

            migrationBuilder.DropTable(
                name: "QualityControls");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PendingForPayments");

            migrationBuilder.DropTable(
                name: "MrpItems");

            migrationBuilder.DropTable(
                name: "PRContractSubjects");

            migrationBuilder.DropTable(
                name: "ConfirmationWorkFlows");

            migrationBuilder.DropTable(
                name: "RevisionActivityTimesheets");

            migrationBuilder.DropTable(
                name: "RevisionComments");

            migrationBuilder.DropTable(
                name: "RFPSupplierProposals");

            migrationBuilder.DropTable(
                name: "RFPComments");

            migrationBuilder.DropTable(
                name: "PurchaseRequestItems");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "TeamWorkUsers");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Transmittals");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SCMAuditLogs");

            migrationBuilder.DropTable(
                name: "ReceiptRejects");

            migrationBuilder.DropTable(
                name: "CommunicationQuestions");

            migrationBuilder.DropTable(
                name: "POPreparations");

            migrationBuilder.DropTable(
                name: "PoTermsOfPayments");

            migrationBuilder.DropTable(
                name: "RevisionActivities");

            migrationBuilder.DropTable(
                name: "RFPInqueries");

            migrationBuilder.DropTable(
                name: "RFPSuppliers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PurchaseRequests");

            migrationBuilder.DropTable(
                name: "TeamWorks");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "DocumentCommunications");

            migrationBuilder.DropTable(
                name: "DocumentTQNCRs");

            migrationBuilder.DropTable(
                name: "ProductGroups");

            migrationBuilder.DropTable(
                name: "Mrps");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Packs");

            migrationBuilder.DropTable(
                name: "DocumentRevisions");

            migrationBuilder.DropTable(
                name: "POs");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "PRContracts");

            migrationBuilder.DropTable(
                name: "ConfirmationWorkFlowTemplates");

            migrationBuilder.DropTable(
                name: "RFPs");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "DocumentGroups");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
