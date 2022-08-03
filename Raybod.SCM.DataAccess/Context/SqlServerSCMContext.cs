using Raybod.SCM.Domain.Configuration;
using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Raybod.SCM.DataAccess.Core;
using System.Threading.Tasks;
using System.Threading;
using Raybod.SCM.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using Raybod.SCM.Domain.View;
using System.Linq;

namespace Raybod.SCM.DataAccess.Context
{
    public class SqlServerSCMContext : DbContext, IUnitOfWork
    {
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductUnit> ProductUnits { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierProductGroup> SupplierProductGroups { get; set; }

        public DbSet<CompanyUser> CompanyUsers { get; set; }

        public DbSet<PDFTemplate> PDFTemplates { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseProduct> WarehouseProducts { get; set; }
        public DbSet<ReceiptReject> ReceiptRejects { get; set; }
        public DbSet<ReceiptRejectSubject> ReceiptRejectSubjects { get; set; }
        public DbSet<WarehouseProductStockLogs> WarehouseProductStockLogs { get; set; }
        public DbSet<WarehouseDespatch> WarehouseDespatchs { get; set; }

        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptSubject> ReceiptSubjects { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractFormConfig> ContractFormConfigs { get; set; }
        public DbSet<ContractAttachment> ContractAttachments { get; set; }
        public DbSet<DocumentGroup> DocumentGroups { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentProduct> DocumentProducts { get; set; }
        public DbSet<DocumentRevision> DocumentRevisions { get; set; }
        public DbSet<RevisionActivity> RevisionActivities { get; set; }
        public DbSet<RevisionActivityTimesheet> RevisionActivityTimesheets { get; set; }
        public DbSet<RevisionComment> RevisionComments { get; set; }
        public DbSet<RevisionCommentUser> RevisionCommentUsers { get; set; }
        public DbSet<RevisionAttachment> RevisionAttachments { get; set; }

        public DbSet<Transmittal> Transmittals { get; set; }
        public DbSet<TransmittalRevision> TransmittalRevisions { get; set; }

        public DbSet<DocumentCommunication> DocumentCommunications { get; set; }
        public DbSet<DocumentTQNCR> DocumentTQNCRs { get; set; }
        public DbSet<CommunicationQuestion> CommunicationQuestions { get; set; }
        public DbSet<CommunicationReply> CommunicationReplys { get; set; }
        public DbSet<CommunicationAttachment> CommunicationAttachments { get; set; }
        public DbSet<CommunicationTeamComment> CommunicationTeamComments { get; set; }
        public DbSet<CommunicationTeamCommentUser> CommunicationTeamCommentUsers { get; set; }


        public DbSet<BomProduct> BomProducts { get; set; }

        public DbSet<MasterMR> MasterMRs { get; set; }
        public DbSet<Mrp> Mrps { get; set; }
        public DbSet<MrpItem> MrpItems { get; set; }

        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }

        public DbSet<RFP> RFPs { get; set; }
        public DbSet<RFPAttachment> RFPAttachments { get; set; }
        public DbSet<RFPSupplier> RFPSuppliers { get; set; }
        public DbSet<RFPInquery> RFPInqueries { get; set; }
        public DbSet<RFPSupplierProposal> RFPSupplierProposals { get; set; }
        public DbSet<RFPComment> RFPComments { get; set; }
        public DbSet<RFPCommentInquery> RFPCommentInqueries { get; set; }
        public DbSet<RFPCommentUser> RFPCommentUsers { get; set; }
        public DbSet<RFPStatusLog> RFPStatusLogs { get; set; }

        public DbSet<TeamWork> TeamWorks { get; set; }
        public DbSet<TeamWorkUser> TeamWorkUsers { get; set; }
        public DbSet<UserLatestTeamWork> UserLatestTeamWorks { get; set; }
        public DbSet<TeamWorkUserRole> TeamWorkUserRoles { get; set; }
        public DbSet<TeamWorkUserProductGroup> TeamWorkUserProductGroups { get; set; }
        public DbSet<TeamWorkUserDocumentGroup> TeamWorkUserDocumentGroups { get; set; }

        public DbSet<PRContract> PRContracts { get; set; }
        public DbSet<PRContractSubject> PRContractSubjects { get; set; }


        public DbSet<PAttachment> PAttachments { get; set; }

        public DbSet<PO> POs { get; set; }
        public DbSet<POSubject> POSubjects { get; set; }
        public DbSet<POTermsOfPayment> PoTermsOfPayments { get; set; }
        public DbSet<POStatusLog> POStatusLogs { get; set; }
        public DbSet<QualityControl> QualityControls { get; set; }
        public DbSet<POComment> POComments { get; set; }
        public DbSet<POCommentUser> POCommentUsers { get; set; }
        public DbSet<POActivity> POActivities { get; set; }
        public DbSet<POActivityTimesheet> POActivityTimesheets { get; set; }
        public DbSet<PoProgress> POProgresses { get; set; }

        public DbSet<Pack> Packs { get; set; }
        public DbSet<PackSubject> PackingSubjects { get; set; }

        public DbSet<Logistic> Logistics { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceProduct> InvoiceProducts { get; set; }
        public DbSet<PendingForPayment> PendingForPayments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentPendingForPayment> PaymentPendingForPayments { get; set; }
        public DbSet<PaymentAttachment> PaymentAttachments { get; set; }
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }



        public DbSet<ConfirmationWorkFlow> ConfirmationWorkFlows { get; set; }
        public DbSet<ConfirmationWorkFlowUser> ConfirmationWorkFlowUsers { get; set; }


        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SCMAuditLog> SCMAuditLogs { get; set; }
        public DbSet<LogUserReceiver> LogUserReceivers { get; set; }
        public DbSet<UserSeenScmAuditLog> UserSeenSCMAuditLogs { get; set; }
        public DbSet<UserPinAuditLog> UserPinAuditLogs { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Area> Areas { get; set; }

        public DbSet<OperationGroup> OperationGroups { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<StatusOperation> OperationStatuses { get; set; }
        public DbSet<OperationProgress> OperationProgresses { get; set; }
        public DbSet<OperationAttachment> OperationAttachments { get; set; }
        public DbSet<OperationComment> OperationComments { get; set; }
        public DbSet<OperationCommentUser> OperationCommentUsers { get; set; }
        public DbSet<OperationActivity> OperationActivities { get; set; }
        public DbSet<OperationActivityTimesheet> OperationActivityTimesheets { get; set; }
        public DbSet<FileDriveDirectory> FileDriveDirectories { get; set; }
        public DbSet<FileDriveFile> fileDriveFiles { get; set; }
        public DbSet<FileDriveShare> fileDriveShares { get; set; }
        public DbSet<PlanService> PlanServices { get; set; }
        public DbSet<TeamWorkUserOperationGroup> TeamWorkUserOperationGroups { get; set; }
        public DbSet<PurchaseConfirmationWorkFlow> PurchaseRequestConfirmationWorkFlows { get; set; }
        public DbSet<PurchaseConfirmationWorkFlowUser> PurchaseRequestConfirmationWorkFlowUsers { get; set; }
        public DbSet<PrContractConfirmationWorkFlow> PrContractConfirmationWorkFlows { get; set; }
        public DbSet<PrContractConfirmationWorkFlowUser> PrContractConfirmationWorkFlowUsers { get; set; }
        public DbSet<RFPProForma> RFPProFormas { get; set; }

        public DbSet<POInspection> POInspections { get; set; }
        public DbSet<PoSupplierDocument> POSupplierDocuments { get; set; }
        public DbSet<WarehouseOutputRequest> WarehouseOutputRequests { get; set; }
        public DbSet<WarehouseOutputRequestSubject> WarehouseOutputRequestSubjects { get; set; }
        public DbSet<WarehouseOutputRequestWorkFlow> WarehouseOutputRequestWorkFlows { get; set; }
        public DbSet<WarehouseOutputRequestWorkFlowUser> WarehouseOutputRequestWorkFlowUsers { get; set; }
        public DbSet<PaymentConfirmationWorkFlow> PaymentConfirmationWorkFlows { get; set; }
        public DbSet<PaymentConfirmationWorkFlowUser> PaymentConfirmationWorkFlowUsers { get; set; }
        public DbSet<EmailErrorLog> EmailErrorLogs { get; set; }
        public DbSet<UserMentions> UserMentions { get; set; }
        public DbSet<FileDriveComment> FileDriveComments { get; set; }
        public DbSet<FileDriveCommentUser> FileDriveCommentUsers { get; set; }
        public DbSet<FileDriveCommentAttachment> FileDriveCommentAttachments { get; set; }
        public DbSet<UserNotify> UserNotifies { get; set; }


        // sql view 
        public DbQuery<FinancialAccountBaseOnSupplier> FinancialAccountBaseOnSuppliers { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        public SqlServerSCMContext(DbContextOptions<SqlServerSCMContext> options, IHttpContextAccessor httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // view 
            modelBuilder.Query<FinancialAccountBaseOnSupplier>().ToView("V_FinancialAccountBaseOnSupplier");
            //configuration
            modelBuilder.ApplyConfiguration(new BomProductConfiguration());
            modelBuilder.ApplyConfiguration(new CommunicationQuestionConfiguration());
            modelBuilder.ApplyConfiguration(new CommunicationReplyConfiguration());
            modelBuilder.ApplyConfiguration(new CommunicationTeamCommentConfiguration());
            modelBuilder.ApplyConfiguration(new CommunicationTeamCommentUserConfiguration());
            modelBuilder.ApplyConfiguration(new ConfirmationWorkFlowConfiguration());
            modelBuilder.ApplyConfiguration(new ConfirmationWorkFlowUserConfiguration());
            modelBuilder.ApplyConfiguration(new ContractAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new ContractConfiguration());
            modelBuilder.ApplyConfiguration(new ContractFormConfigCofiguration());
            modelBuilder.ApplyConfiguration(new DocumentConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentRevisionConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentCommunicationConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentTQNCRConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentProductConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceProductConfiguration());
            modelBuilder.ApplyConfiguration(new LogisticConfiguration());
            modelBuilder.ApplyConfiguration(new LogUserReceiverConfiguration());
            modelBuilder.ApplyConfiguration(new MasterMRConfiguration());
            modelBuilder.ApplyConfiguration(new MrpConfiguration());
            modelBuilder.ApplyConfiguration(new MrpItemConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new PackConfiguration());
            modelBuilder.ApplyConfiguration(new PackSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new PendingForPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new POConfiguration());
            modelBuilder.ApplyConfiguration(new POCommentConfiguration());
            modelBuilder.ApplyConfiguration(new POCommentUserConfiguration());
            //modelBuilder.ApplyConfiguration(new POServiceConfiguration());
            modelBuilder.ApplyConfiguration(new POSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new POTermsOfPaymentConfiguration());
            modelBuilder.ApplyConfiguration(new POActivityConfiguration());
            modelBuilder.ApplyConfiguration(new POActivityTimesheetConfiguration());
            modelBuilder.ApplyConfiguration(new PRContractConfiguration());
            modelBuilder.ApplyConfiguration(new PRContractSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductGroupConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseRequestConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseRequestItemConfiguration());
            modelBuilder.ApplyConfiguration(new QualityControlConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptRejectConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptRejectSubjectConfiguration());
            modelBuilder.ApplyConfiguration(new RevisionActivityConfiguration());
            modelBuilder.ApplyConfiguration(new RevisionActivityTimesheetConfiguration());
            modelBuilder.ApplyConfiguration(new RevisionCommentConfiguration());
            modelBuilder.ApplyConfiguration(new RevisionCommentUserConfiguration());
            modelBuilder.ApplyConfiguration(new RFPConfiguration());
            modelBuilder.ApplyConfiguration(new RFPInqueryConfiguration());
            modelBuilder.ApplyConfiguration(new RFPItemsConfiguration());
            modelBuilder.ApplyConfiguration(new RFPSupplierConfiguration());
            modelBuilder.ApplyConfiguration(new RFPSupplierProposalConfiguration());
            modelBuilder.ApplyConfiguration(new RFPCommentConfiguration());
            modelBuilder.ApplyConfiguration(new RFPCommentInqueryConfiguration());
            modelBuilder.ApplyConfiguration(new RFPCommentUserConfiguration());
            modelBuilder.ApplyConfiguration(new RFPAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new SCMAuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new SupplierConfiguration());
            modelBuilder.ApplyConfiguration(new SupplierProductGroupConfiguration());
            modelBuilder.ApplyConfiguration(new TransmittalConfiguration());
            modelBuilder.ApplyConfiguration(new TransmittalRevisionConfiguration());
            modelBuilder.ApplyConfiguration(new TeamWorkUserDocumentGroupConfiguration());
            modelBuilder.ApplyConfiguration(new TeamWorkUserOperationGroupConfiguration());
            modelBuilder.ApplyConfiguration(new TeamWorkUserProductGroupConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserNotificationConfiguration());
            modelBuilder.ApplyConfiguration(new UserSeenSCMAuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseProductConfiguration());
            modelBuilder.ApplyConfiguration(new AreaConfiguration());
            modelBuilder.ApplyConfiguration(new FileDriveCommentConfiguration());
            modelBuilder.ApplyConfiguration(new FileDriveCommentUserConfiguration());

        }

        public virtual int SaveChange()
        {
            var changedEntityNames = this.GetChangedEntityNames();
            OnBeforeSaving();
            this.ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = base.SaveChanges();
            this.ChangeTracker.AutoDetectChangesEnabled = true;

            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

            return result;

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var changedEntityNames = this.GetChangedEntityNames();
            //bool isNeedAudit = true;
            OnBeforeSaving();
            //this.ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
            var result = await base.SaveChangesAsync(cancellationToken);
            this.ChangeTracker.AutoDetectChangesEnabled = true;

            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

            return result;
        }

        //public void Dispose()
        //{
        //    base.Dispose();
        //}
        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            int? userId = null;
            try
            {
                var user = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (user != null)
                    userId = int.Parse(user.Value);
            }
            catch (Exception) { }

            foreach (var entry in entries)
            {
                if (entry.Entity is BaseAuditEntity trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdateDate = now;
                            trackable.ModifierUserId = userId;
                            break;

                        case EntityState.Added:
                            trackable.CreatedDate = now;
                            trackable.UpdateDate = now;
                            trackable.AdderUserId = userId;
                            trackable.ModifierUserId = userId;
                            break;
                    }
                }
            }
        }

        //private List<AuditEntry> OnBeforeSavingAsync()
        //{
        //    var entries = ChangeTracker.Entries();

        //    entries = entries.Where(a => a.State != EntityState.Detached && a.State != EntityState.Unchanged && !(a.Entity is Audit));
        //    if (entries.Count() == 0)
        //        return null;

        //    int? userId = null;
        //    string Controller = null;
        //    string Action = null;
        //    string username = null;
        //    try
        //    {
        //        var user = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        //        if (user != null)
        //            userId = int.Parse(user.Value);

        //        var surname = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Surname);
        //        if (surname != null)
        //            username = surname.Value;

        //        var rd = _httpContextAccessor.HttpContext.GetRouteData();
        //        Controller = rd.Values["controller"].ToString();
        //        Action = rd.Values["action"].ToString();
        //    }
        //    catch (Exception) { }

        //    var auditEntries = new List<AuditEntry>();
        //    foreach (var entry in entries)
        //    {
        //        if (entry.Entity is BaseAuditEntity trackable)
        //        {

        //            var now = DateTime.UtcNow;
        //            switch (entry.State)
        //            {
        //                case EntityState.Modified:
        //                    trackable.UpdateDate = now;
        //                    trackable.ModifierUserId = userId;
        //                    break;

        //                case EntityState.Added:
        //                    trackable.CreatedDate = now;
        //                    trackable.UpdateDate = now;
        //                    trackable.AdderUserId = userId;
        //                    trackable.ModifierUserId = userId;
        //                    break;
        //            }
        //        }

        //        var auditEntry = new AuditEntry(entry);
        //        auditEntry.TableName = entry.Metadata.GetTableName();
        //        auditEntry.Username = username;
        //        auditEntry.Controller = Controller;
        //        auditEntry.Action = Action;
        //        auditEntry.UserId = userId;
        //        auditEntry.AuditType = entry.State.ToString();
        //        auditEntries.Add(auditEntry);
        //        foreach (var item in entry.Properties.Where(a => a.IsTemporary))
        //        {
        //            // value will be generated by the database, get the value after saving
        //            auditEntry.TemporaryProperties.Add(item);
        //        }
        //        var keys = entry.Properties
        //        .Where(a => a.Metadata.IsPrimaryKey())
        //        .Select(p => p.CurrentValue)
        //        .ToList();
        //        auditEntry.KeyValues = ReturnStringKeyValue(keys);

        //        var foreignKeys = entry.Properties
        //        .Where(a => a.Metadata.IsForeignKey() && a.Metadata.Name != "AdderUserId" && a.Metadata.Name != "ModifierUserId" && a.CurrentValue != null)
        //        .Select(p => p.CurrentValue)
        //        .ToList();
        //        auditEntry.ForeignKey = ReturnStringKeyValue(foreignKeys);

        //        switch (entry.State)
        //        {
        //            case EntityState.Added:
        //                auditEntry.NewValues = entry.Properties
        //                    .Where(a => !a.IsTemporary)
        //                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
        //                    ?? new Dictionary<string, object>();
        //                break;

        //            case EntityState.Deleted:
        //                auditEntry.OldValues = entry.Properties
        //                    .ToDictionary(a => a.Metadata.Name, a => a.OriginalValue)
        //                    ?? new Dictionary<string, object>();
        //                break;

        //            case EntityState.Modified:
        //                auditEntry.OldValues = entry.Properties
        //                .ToDictionary(a => a.Metadata.Name, a => a.OriginalValue)
        //                ?? new Dictionary<string, object>();
        //                auditEntry.NewValues = entry.Properties
        //                .ToDictionary(a => a.Metadata.Name, a => a.CurrentValue)
        //                ?? new Dictionary<string, object>();
        //                break;
        //        }
        //    }
        //    // Save audit entities that have all the modifications
        //    foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        //    {
        //        Audits.Add(auditEntry.ToAudit());
        //    }

        //    // keep a list of entries where the value of some properties are unknown at this step
        //    return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        //}



        //private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        //{
        //    if (auditEntries == null || auditEntries.Count == 0)
        //        return Task.CompletedTask;

        //    foreach (var auditEntry in auditEntries)
        //    {
        //        // Get the final value of the temporary properties
        //        foreach (var prop in auditEntry.TemporaryProperties)
        //        {
        //            if (prop.Metadata.IsPrimaryKey())
        //            {
        //                auditEntry.KeyValues = prop.CurrentValue.ToString();
        //            }
        //            else
        //            {
        //                auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
        //            }
        //        }

        //        // Save the Audit entry
        //        Audits.Add(auditEntry.ToAudit());
        //    }

        //    return SaveChangesAsync();
        //}

        //public string ReturnStringKeyValue(List<object> keys)
        //{
        //    if (keys == null || keys.Count() == 0)
        //        return null;
        //    if (keys.Count() == 1)
        //    {
        //        return keys.First().ToString();
        //    }
        //    string keyValues = string.Empty;
        //    foreach (var key in keys)
        //    {
        //        keyValues += key.ToString() + ",";
        //    }
        //    return keyValues;
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    if (modelBuilder == null)
        //        throw new ArgumentNullException(nameof(modelBuilder));

        //    //DbInterception.Add(new YeKeInterceptor());
        //    // در namespace میگرده همه رو ازشون dbset درست میکنه
        //    // dbset be ezaye tamame entity ha automatic misaze kafiye ye duneasho taypeof bedi assembly begiri maslan car ya customer yekish kafis
        //    //LoadEntities پایین تر تعریف شده 

        //    LoadEntities(typeof(Company).GetTypeInfo().Assembly, modelBuilder, "Domain.Model");
        //    ApplyAllConfigurationsFromCurrentAssembly(modelBuilder, typeof(Company).GetTypeInfo().Assembly, "Domain.Model");
        //    base.OnModelCreating(modelBuilder);
        //}



        //#region AutoRegisterEntityType

        ///// <summary>
        ///// رجیستر کردن خودکار موجودیت ها در کانتکس با استفاده از رفلکشن
        ///// </summary>
        ///// <param name="asm">اسمبلی ای که باید برای موجودیت ها در آن بگردد</param>
        ///// <param name="modelBuilder"></param>
        ///// <param name="nameSpace">فضای نامی موجودیت هایی که باید رجیستر شوند</param>
        //public void LoadEntities(Assembly asm, ModelBuilder modelBuilder, string nameSpace)
        //{
        //    var modelInAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(asm.Location);
        //    // var modelInAssembly = Assembly.Load(new AssemblyName("ModuleApp"));
        //    var entityMethod = typeof(ModelBuilder).GetMethod("Entity", new Type[] { });
        //    foreach (var type in modelInAssembly.ExportedTypes)
        //    {
        //        if (type.BaseType is System.Object && !type.IsAbstract && type.Namespace == nameSpace)
        //        {
        //            entityMethod.MakeGenericMethod(type).Invoke(modelBuilder, new object[] { });
        //        }
        //    }
        //}
        //#endregion

        ///// <summary>
        /////رجیستر کردن فایل های پیکربندی موجودیت ها FluentApi
        ///// </summary>
        ///// <param name="modelBuilder"></param>
        ///// <param name="assembly"></param>
        ///// <param name="configNamespace"></param>
        //public void ApplyAllConfigurationsFromCurrentAssembly(ModelBuilder modelBuilder, Assembly assembly, string configNamespace = "")
        //{
        //    var applyGenericMethods = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        //    var applyGenericApplyConfigurationMethods = applyGenericMethods.Where(m => m.IsGenericMethod && m.Name.Equals("ApplyConfiguration", StringComparison.OrdinalIgnoreCase));
        //    var applyGenericMethod = applyGenericApplyConfigurationMethods.FirstOrDefault(m => m.GetParameters().FirstOrDefault()?.ParameterType.Name == "IEntityTypeConfiguration`1");


        //    var applicableTypes = assembly
        //        .GetTypes()
        //        .Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters);

        //    if (!string.IsNullOrEmpty(configNamespace))
        //    {
        //        applicableTypes = applicableTypes.Where(c => c.Namespace == configNamespace);
        //    }

        //    foreach (var type in applicableTypes)
        //    {
        //        foreach (var iface in type.GetInterfaces())
        //        {
        //            // if type implements interface IEntityTypeConfiguration<SomeEntity>
        //            if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
        //            {
        //                // make concrete ApplyConfiguration<SomeEntity> method
        //                var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
        //                // and invoke that with fresh instance of your configuration type
        //                applyConcreteMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(type) });
        //                Console.WriteLine("applied model " + type.Name);
        //                break;
        //            }
        //        }
        //    }

        //}


    }
}
