using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject.Mrp;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.DataTransferObject.Contract;
using Raybod.SCM.DataTransferObject.MasterMrpReport;
using Raybod.SCM.DataAccess.Extention;
using Raybod.SCM.Utility.Helpers;

namespace Raybod.SCM.Services.Implementation.Planning
{
    public class MasterMrReportService : IMasterMrReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly DbSet<MasterMR> _masterMRRepository;
        private readonly DbSet<BomProduct> _bomProductRepository;
        private readonly DbSet<MrpItem> _mrpItemRepository;
        private readonly DbSet<PurchaseRequestItem> _prItemRepository;
        private readonly DbSet<RFPItems> _rfpItemRepository;
        private readonly DbSet<PRContractSubject> _prContractSubjectRepository;
        private readonly DbSet<POSubject> _poSubjectRepository;
        private readonly DbSet<Contract> _contractRepository;
        private readonly DbSet<Area> _areaRepository;
        private readonly CompanyAppSettingsDto _appSettings;


        public MasterMrReportService(
            IUnitOfWork unitOfWork,
            IOptions<CompanyAppSettingsDto> AppSettings,
            ITeamWorkAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _masterMRRepository = _unitOfWork.Set<MasterMR>();
            _mrpItemRepository = _unitOfWork.Set<MrpItem>();
            _bomProductRepository = _unitOfWork.Set<BomProduct>();
            _prItemRepository = _unitOfWork.Set<PurchaseRequestItem>();
            _rfpItemRepository = _unitOfWork.Set<RFPItems>();
            _prContractSubjectRepository = _unitOfWork.Set<PRContractSubject>();
            _poSubjectRepository = _unitOfWork.Set<POSubject>();
            _contractRepository = _unitOfWork.Set<Contract>();
            _appSettings = AppSettings.Value;
            _areaRepository = _unitOfWork.Set<Area>();
        }

        //public async Task<ServiceResult<List<MasterMrProductListDto>>> GetMasterMrByContractCodeAsync(AuthenticateDto authenticate, MasterMRQueryDto query)
        //{
        //    try
        //    {
        //        var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
        //        if (!permission.HasPermission)
        //            return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);

        //        var dbQuery = _masterMRRepository
        //            .AsNoTracking()
        //            .Where(a => a.ContractCode == authenticate.ContractCode);
        //        var dbQuerybom = _bomProductRepository
        //          .AsNoTracking()
        //          .Include(c => c.ChildBom)
        //          .Where(x => x.ParentBomId == null && !x.IsDeleted)
        //          .OrderByDescending(x => x.Id)
        //          .AsQueryable();

        //        dbQuerybom = dbQuerybom.Where(a => a.Product.ContractSubjects.Any(c => c.ContractCode == authenticate.ContractCode));


        //        if (permission.ProductGroupIds.Any())
        //            dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

        //        if (!string.IsNullOrEmpty(query.SearchText))
        //            dbQuery = dbQuery.Where(a => a.Product.Description.Contains(query.SearchText) ||
        //                                         a.Product.ProductCode.Contains(query.SearchText));

        //        if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
        //            dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.Product.ProductGroupId));

        //        if (query.AreaIds != null && query.AreaIds.Any())
        //            dbQuery = dbQuery.Where(a =>dbQuerybom.Any(b=>b.ProductId==a.ProductId&&query.AreaIds.Contains(b.AreaId.Value))|| dbQuerybom.Any(b=>b.ChildBom.Any(c=>c.ProductId==a.ProductId&&query.AreaIds.Contains(c.AreaId.Value))));

        //        if (EnumHelper.ValidateItem(query.Status))
        //            dbQuery = query.Status == MrpItemStatus.NotMRP
        //                ? dbQuery.Where(a => !a.MrpItems.Any())
        //                : dbQuery.Where(a => a.MrpItems.Any(c => !c.IsDeleted && c.MrpItemStatus == query.Status));

        //        var totalCount = dbQuery.Count();

        //        dbQuery = dbQuery.ApplayPageing(query);

        //        var result = await dbQuery.Select(a => new MasterMrProductListDto
        //        {
        //            ProductId = a.ProductId,
        //            ProductCode = a.Product.ProductCode,
        //            ProductDescription = a.Product.Description,
        //            ProductTechnicalNumber = a.Product.TechnicalNumber,
        //            ProductGroupTitle = a.Product.ProductGroup.Title,
        //            ProductGroupId = a.Product.ProductGroupId,
        //            Unit = a.Product.Unit,
        //            Quantity = a.GrossRequirement,
        //            Areas= dbQuerybom.SelectMany(b=>b.ChildBom).SelectMany(c => c.ChildBom.Where(d => d.AreaId != null && d.ProductId == a.ProductId)).Select(e => new MasterMrAreaDto { AreaId = e.AreaId.Value, AreaTitle = e.Area.AreaTitle }).GetArea(dbQuerybom.SelectMany(b=>b.ChildBom.Where(c=>c.AreaId!=null&&c.ProductId==a.ProductId).Select(e => new MasterMrAreaDto { AreaId = e.AreaId.Value, AreaTitle = e.Area.AreaTitle }))),
        //            PlannedQuantity = a.GrossRequirement - a.RemainedGrossRequirement,
        //            MrpItemStatus = a.MrpItems.Where(a => !a.IsDeleted)
        //                .OrderByDescending(c => c.MrpItemStatus)
        //                .Select(v => v.MrpItemStatus)
        //                .FirstOrDefault()
        //        }).ToListAsync();



        //        return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
        //    }
        //    catch (Exception exception)
        //    {
        //        return ServiceResultFactory.CreateException<List<MasterMrProductListDto>>(null, exception);
        //    }
        //}
        public async Task<ServiceResult<List<MasterMrProductListDto>>> GetMasterMrByContractCodeAsync(AuthenticateDto authenticate, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MasterMrProductListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .AsNoTracking()
                    .Include(a=>a.Product)
                    .ThenInclude(a=>a.BomProducts)
                    .ThenInclude(a=>a.Area)
                    .Where(a => a.ContractCode == authenticate.ContractCode&&(a.Product.BomProducts.Any(b=>!b.IsDeleted&&b.MaterialType!=MaterialType.Component)));
                var dbQuerybom = _bomProductRepository
                  .AsNoTracking()
                  .Include(a=>a.Product)
                  .Include(a=>a.Area)
                  .Include(c => c.ChildBom)
                  .Where(x => x.ParentBomId == null && !x.IsDeleted)
                  .OrderByDescending(x => x.Id)
                  .AsQueryable();

                dbQuerybom = dbQuerybom.Where(a => a.Product.ContractCode == authenticate.ContractCode);


                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                if (!string.IsNullOrEmpty(query.SearchText))
                    dbQuery = dbQuery.Where(a => a.Product.Description.Contains(query.SearchText) ||
                                                 a.Product.ProductCode.Contains(query.SearchText));

                if (query.ProductGroupIds != null && query.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => query.ProductGroupIds.Contains(a.Product.ProductGroupId));

                if (query.AreaIds != null && query.AreaIds.Any())
                    dbQuery = dbQuery.Where(a => dbQuerybom.Any(b => b.ProductId == a.ProductId && query.AreaIds.Contains(b.AreaId.Value)) || dbQuerybom.Any(b => b.ChildBom.Any(c => c.ProductId == a.ProductId && query.AreaIds.Contains(c.AreaId.Value))));

                if (EnumHelper.ValidateItem(query.Status))
                    dbQuery = query.Status == MrpItemStatus.NotMRP
                        ? dbQuery.Where(a => !a.MrpItems.Any())
                        : dbQuery.Where(a => a.MrpItems.Any(c => !c.IsDeleted && c.MrpItemStatus == query.Status));

                var totalCount = dbQuery.Count();

                dbQuery = dbQuery.ApplayPageing(query);
               var setWithNoSubset=await dbQuerybom.Where(a=>a.ParentBom==null&&a.MaterialType==MaterialType.Component&&(a.ChildBom==null||!a.ChildBom.Any())).Select(a => new MasterMrProductListDto
                {
                    ProductId = a.ProductId,
                    ProductCode = a.Product.ProductCode,
                    ProductDescription = a.Product.Description,
                    ProductTechnicalNumber = a.Product.TechnicalNumber,
                    ProductGroupTitle = a.Product.ProductGroup.Title,
                    ProductGroupId = a.Product.ProductGroupId,
                    Unit = a.Product.Unit,
                    Quantity = a.CoefficientUse,
                    Areas = (a.Area != null)? new List<MasterMrAreaDto> { new MasterMrAreaDto { AreaId = a.Area.AreaId,AreaTitle=a.Area.AreaTitle } }:new List<MasterMrAreaDto>(),
                    PlannedQuantity = 0,
                    MrpItemStatus =  MrpItemStatus.SetWithoutSebset,
                   DocumentStatus =
                            !a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
               }).ToListAsync();
                var result = await dbQuery.Select(a => new MasterMrProductListDto
                {
                    ProductId = a.ProductId,
                    ProductCode = a.Product.ProductCode,
                    ProductDescription = a.Product.Description,
                    ProductTechnicalNumber = a.Product.TechnicalNumber,
                    ProductGroupTitle = a.Product.ProductGroup.Title,
                    ProductGroupId = a.Product.ProductGroupId,
                    Unit = a.Product.Unit,
                    Quantity = a.GrossRequirement,
                    Areas = DistinctArea(a.Product.BomProducts.Where(b=>!b.IsDeleted&&b.MaterialType!= MaterialType.Component&&b.Area!=null).Select(b=>new MasterMrAreaDto {AreaId=b.Area.AreaId,AreaTitle=b.Area.AreaTitle }).Distinct().ToList()),
                    PlannedQuantity = a.GrossRequirement - a.RemainedGrossRequirement,
                    MrpItemStatus =(a.Product.BomProducts.Any(b=>!b.IsDeleted&&b.MaterialType==MaterialType.Component)&&!a.Product.BomProducts.Any(b => !b.IsDeleted && b.MaterialType != MaterialType.Component))?MrpItemStatus.SetWithoutSebset : a.MrpItems.Where(a => !a.IsDeleted)
                        .OrderByDescending(c => c.MrpItemStatus)
                        .Select(v => v.MrpItemStatus)
                        .FirstOrDefault(),
                    DocumentStatus =
                            !a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                }).ToListAsync();

                if (setWithNoSubset != null && setWithNoSubset.Any())
                    result.AddRange(setWithNoSubset);

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MasterMrProductListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<MasterMrProductListDto>> GetMasterMrDetailByProductIdAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<MasterMrProductListDto>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .AsNoTracking()
                    .Include(a => a.Product)
                    .ThenInclude(a => a.BomProducts)
                    .ThenInclude(a => a.Area)
                    .Where(a => a.ContractCode == authenticate.ContractCode && a.ProductId==productId);

                var result = await dbQuery.Select(a => new MasterMrProductListDto
                {
                    ProductId = a.ProductId,
                    ProductCode = a.Product.ProductCode,
                    ProductDescription = a.Product.Description,
                    ProductTechnicalNumber = a.Product.TechnicalNumber,
                    ProductGroupTitle = a.Product.ProductGroup.Title,
                    ProductGroupId = a.Product.ProductGroupId,
                    Unit = a.Product.Unit,
                    Quantity = a.GrossRequirement,
                    Areas = a.Product.BomProducts.Where(b => b.Area != null).Select(b => new MasterMrAreaDto { AreaId = b.Area.AreaId, AreaTitle = b.Area.AreaTitle }).Distinct().ToList(),
                    PlannedQuantity = a.GrossRequirement - a.RemainedGrossRequirement,
                    MrpItemStatus = (a.Product.BomProducts.Any(b => !b.IsDeleted && b.MaterialType == MaterialType.Component) && !a.Product.BomProducts.Any(b => !b.IsDeleted && b.MaterialType != MaterialType.Component)) ? MrpItemStatus.SetWithoutSebset : a.MrpItems.Where(a => !a.IsDeleted)
                        .OrderByDescending(c => c.MrpItemStatus)
                        .Select(v => v.MrpItemStatus)
                        .FirstOrDefault(),
                    DocumentStatus =
                            !a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : a.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                }).FirstOrDefaultAsync();

               

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<MasterMrProductListDto>(null, exception);
            }
        }
        public async Task<ServiceResult<MasterMrProductDetailsDto>> GetMasterMrByProductIdAsync(AuthenticateDto authenticate, int productId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<MasterMrProductDetailsDto>(null, MessageId.AccessDenied);

                var dbQuery = _masterMRRepository
                    .AsNoTracking()
                    .Where(a => a.ContractCode == authenticate.ContractCode &&
                     a.ProductId == productId);

                if (permission.ProductGroupIds.Any() && !dbQuery.Any(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId)))
                    return ServiceResultFactory.CreateError<MasterMrProductDetailsDto>(null, MessageId.AccessDenied);

                var result = await dbQuery.Select(c => new MasterMrProductDetailsDto
                {
                    ProductId = c.ProductId,
                    ProductCode = c.Product.ProductCode,
                    ProductDescription = c.Product.Description,
                    ProductTechnicalNumber = c.Product.TechnicalNumber,
                    ProductGroupTitle = c.Product.ProductGroup.Title,
                    ProductGroupId = c.Product.ProductGroupId,
                    Unit = c.Product.Unit,
                    Quantity = c.GrossRequirement,
                    PlannedQuantity = c.GrossRequirement - c.RemainedGrossRequirement,
                    MrpItemStatus = c.MrpItems.Where(a => !a.IsDeleted)
                        .OrderByDescending(c => c.MrpItemStatus)
                        .Select(v => v.MrpItemStatus)
                        .FirstOrDefault(),
                    DocumentStatus =
                            !c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode)
                            ? EngineeringDocumentStatus.No
                            : c.Product.DocumentProducts.Any(v => !v.Document.IsDeleted && v.Document.IsActive && v.Document.ContractCode == authenticate.ContractCode &&
                            (!v.Document.DocumentRevisions.Any(a => !a.IsDeleted) || v.Document.DocumentRevisions.Any(n => !n.IsDeleted && n.RevisionStatus < RevisionStatus.Confirmed)))
                            ? EngineeringDocumentStatus.completing
                            : EngineeringDocumentStatus.Completed,
                }).FirstOrDefaultAsync();

                if (result == null)
                    return ServiceResultFactory.CreateError(new MasterMrProductDetailsDto(), MessageId.EntityDoesNotExist);

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<MasterMrProductDetailsDto>(null, exception);
            }
        }

        public async Task<ServiceResult<List<MrpReportListDto>>> GetMrpReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<MrpReportListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _mrpItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.ProductId == productId &&
                    !a.Mrp.IsDeleted &&
                    a.Mrp.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(m => new MrpReportListDto
                {
                    MRPItemId = m.Id,
                    MRPId = m.MrpId,
                    CreateDate = m.Mrp.CreatedDate.ToUnixTimestamp(),
                    DateEnd = m.DateEnd.ToUnixTimestamp(),
                    DateStart = m.DateStart.ToUnixTimestamp(),
                    GrossRequirement = m.GrossRequirement,
                    PO = m.PO,
                    ReservedStock = m.ReservedStock,
                    WarehouseStock = m.WarehouseStock,
                    SurplusQuantity = m.SurplusQuantity,
                    MrpNumber = m.Mrp.MrpNumber,

                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<MrpReportListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PRReportListDto>>> GetPRReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PRReportListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.ProductId == productId &&
                    !a.PurchaseRequest.IsDeleted &&
                    a.PurchaseRequest.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(a => new PRReportListDto
                {
                    CreateDate = a.PurchaseRequest.CreatedDate.ToUnixTimestamp(),
                    MRPNumber = a.PurchaseRequest.Mrp.MrpNumber,
                    PRCode = a.PurchaseRequest.PRCode,
                    PRId = a.PurchaseRequestId,
                    PRItemId = a.Id,
                    Quntity = a.Quntity
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PRReportListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<RFPReportListDto>>> GetRFPReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<RFPReportListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _rfpItemRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.IsActive &&
                    a.ProductId == productId &&
                    !a.RFP.IsDeleted &&
                    a.RFP.ContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(a => new RFPReportListDto
                {
                    RFPId = a.RFPId,
                    RFPItemId = a.Id,
                    RFPStatus = a.RFP.Status,
                    CreateDate = a.RFP.CreatedDate.ToUnixTimestamp(),
                    RFPNumber = a.RFP.RFPNumber,
                    PRCode = a.PurchaseRequestItem.PurchaseRequest.PRCode,
                    Quntity = a.Quantity,
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<RFPReportListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<PRCReportListDto>>> GetPRContractReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<PRCReportListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _prContractSubjectRepository
                    .AsNoTracking()
                    .Where(a => !a.IsDeleted &&
                    a.ProductId == productId &&
                    a.PRContract.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(a => new PRCReportListDto
                {
                    PRCSubjectId = a.Id,
                    DateEnd = a.PRContract.DateEnd.ToUnixTimestamp(),
                    DateIssued = a.PRContract.DateIssued.ToUnixTimestamp(),
                    CreateDate = a.PRContract.CreatedDate.ToUnixTimestamp(),
                    RFPNumber = a.RFPItem.RFP.RFPNumber,
                    PRContractCode = a.PRContract.PRContractCode,
                    PRContractId = a.PRContractId,
                    Quntity = a.Quantity,
                    OrderQuantity = a.Product.POSubjects.Where(c => c.PO.POStatus != POStatus.Pending && c.PO.PRContractId == a.PRContractId).Sum(v => v.Quantity),
                    ReceiptQuantity = a.Product.POSubjects.Where(c => c.PO.POStatus != POStatus.Pending && c.PO.PRContractId == a.PRContractId).Sum(v => v.ReceiptedQuantity),
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<PRCReportListDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<List<POReportListDto>>> GetPOReportListByProductIdAsync(AuthenticateDto authenticate, int productId, MasterMRQueryDto query)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<POReportListDto>>(null, MessageId.AccessDenied);

                var dbQuery = _poSubjectRepository
                    .AsNoTracking()
                    .Where(a => !a.PO.IsDeleted &&
                    a.ProductId == productId &&
                    a.PO.POStatus != POStatus.Pending &&
                    a.PO.BaseContractCode == authenticate.ContractCode);

                if (permission.ProductGroupIds.Any())
                    dbQuery = dbQuery.Where(a => permission.ProductGroupIds.Contains(a.Product.ProductGroupId));

                var totalCount = dbQuery.Count();
                dbQuery = dbQuery.ApplayPageing(query);

                var result = await dbQuery.Select(a => new POReportListDto
                {
                    POId = a.POId.Value,
                    DateDelivery = a.PO.DateDelivery.ToUnixTimestamp(),
                    MrpNumber = a.MrpItem.Mrp.MrpNumber,
                    POCode = a.PO.POCode,
                    POSubjectId = a.POSubjectId,
                    PRContractCode = a.PO.PRContract.PRContractCode,
                    Quantity = a.Quantity,
                    ReceiptedQuantity = a.ReceiptedQuantity,
                    RemainedQuantity = a.RemainedQuantity,
                    CreateDate = a.PO.CreatedDate.ToUnixTimestamp(),
                }).ToListAsync();

                return ServiceResultFactory.CreateSuccess(result).WithTotalCount(totalCount);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<POReportListDto>>(null, exception);
            }
        }

        
        private static List<MasterMrAreaDto> DistinctArea(List<MasterMrAreaDto> areas)
        {
            List<MasterMrAreaDto> result = new List<MasterMrAreaDto>();
            foreach(var item in areas)
            {
                if (!result.Any(a => a.AreaId == item.AreaId))
                {
                    result.Add(new MasterMrAreaDto
                    {
                        AreaId = item.AreaId,
                        AreaTitle = item.AreaTitle
                    });
                }
            }
            return result;
        }


    }
}
