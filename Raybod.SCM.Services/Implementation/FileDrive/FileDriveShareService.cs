using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
using Raybod.SCM.DataTransferObject.FileDriveShare;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using Raybod.SCM.Utility.FileHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class FileDriveShareService : IFileDriveShareService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<FileDriveShare> _shareRepository;
        private readonly DbSet<FileDriveDirectory> _directoryRepository;
        private readonly DbSet<FileDriveFile> _fileRepository;
        private readonly DbSet<User> _userRepository;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;
        public FileDriveShareService(IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> AppSettings,
            IWebHostEnvironment hostingEnvironmentRoot)
        {
            _unitOfWork = unitOfWork;
            _shareRepository = _unitOfWork.Set<FileDriveShare>();
            _directoryRepository = _unitOfWork.Set<FileDriveDirectory>();
            _fileRepository = _unitOfWork.Set<FileDriveFile>();
            _userRepository = _unitOfWork.Set<User>();
            _authenticationService = authenticationService;
            _appSettings = AppSettings.Value;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
        }


        public async Task<ServiceResult<bool>> AddShareAsync(AuthenticateDto authenticate, FileDriveShareCreateDto model)
        {
            try
            {

                    if(model.EntityType==EntityType.Directory&&!await _shareRepository.AnyAsync(a=>!a.IsDeleted&&a.DirectoryId==model.EntityId&&a.UserId==authenticate.UserId&&a.Accessablity==Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                    if (model.EntityType == EntityType.File && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == model.EntityId && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
        
                    

                List<FileDriveShare> result = new List<FileDriveShare>();
             
                if ((model.EntityType==EntityType.Directory&&await _shareRepository.AnyAsync(a =>  a.DirectoryId == model.EntityId))|| (model.EntityType == EntityType.File && await _shareRepository.AnyAsync(a => a.FileId == model.EntityId && a.Status == ShareEntityStatus.Active)))
                {
                    var share =(model.EntityType==EntityType.Directory)?await _shareRepository.Where(a => a.DirectoryId == model.EntityId && a.Status == ShareEntityStatus.Active).ToListAsync(): await _shareRepository.Where(a => a.FileId == model.EntityId && a.Status == ShareEntityStatus.Active).ToListAsync();
                    List<FileDriveShare> extraShare = new List<FileDriveShare>();

            
                    foreach (var item in model.Owners)
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId=item.UserId,Access=new Access { Value=item.Accessablity,Label=""} });
                    }
                    List<FileDriveShareUserAccessablityDto> deletedItems = new List<FileDriveShareUserAccessablityDto>();
                    foreach(var item in share)
                    {
                        if (!item.IsDeleted && !model.Users.Any(a => a.UserId == item.UserId))
                        {
                            deletedItems.Add(new FileDriveShareUserAccessablityDto { UserId = item.UserId, Access = new Access { Label = "", Value = item.Accessablity } });
                        }
                    }
                    foreach (var item in share)
                        item.IsDeleted = true;
                    foreach (var item in model.Users)
                    {
                       var index= share.FindIndex(a => a.UserId == item.UserId);
                        if(index>=0)
                        {
                            share[index].IsDeleted = false;
                            share[index].Accessablity = item.Access.Value;
                        }
                        else
                        {
                            if (model.EntityType == EntityType.Directory)
                            {

                                await AddSharingRecordForAllSubDirectory(model.EntityId,  new List<FileDriveShareUserAccessablityDto> { item});
                            }
                               
                            else
                                extraShare.Add(new FileDriveShare { Accessablity = item.Access.Value, FileId = model.EntityId, EntityType = model.EntityType, UserId = item.UserId,IsDeleted=false });
                        }
                    }
                    if(deletedItems.Any())
                        await RemoveSharingRecordForAllSubDirectory(model.EntityId, deletedItems);
                    if (extraShare.Count() > 0)
                       await _shareRepository.AddRangeAsync(extraShare);

                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {
                        return ServiceResultFactory.CreateSuccess(true);
                    }

                    return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

                }
               
                if (model.EntityType == EntityType.Directory)
                {
                   
                    
                    var dbQuery = _directoryRepository.Where(a => !a.IsDeleted && a.DirectoryId == model.EntityId && a.ContractCode == authenticate.ContractCode);
                   
                    var directory = await dbQuery.FirstOrDefaultAsync();
                   
                    if (directory == null)
                        return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                    if (directory.AdderUserId != authenticate.UserId&&directory.UserId!=authenticate.UserId)
                        return ServiceResultFactory.CreateError(false, MessageId.OnlyOwnerCanShare);
                    if (directory.AdderUserId == authenticate.UserId)
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = authenticate.UserId, Access = new Access { Label = "", Value = Accessablity.Editor } });
                    }
                    else
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = directory.AdderUserId.Value, Access = new Access { Label = "", Value = Accessablity.Editor } });
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = authenticate.UserId, Access = new Access { Label = "", Value = Accessablity.Editor } });
                    }
                    await  AddSharingRecordForAllSubDirectory(model.EntityId, model.Users);

                }
                else if (model.EntityType == EntityType.File)
                {
                   
                    var dbQuery = _fileRepository.Where(a => !a.IsDeleted && a.FileId == model.EntityId && a.Directory.ContractCode == authenticate.ContractCode);

                    var file = await dbQuery.FirstOrDefaultAsync();
                    if (file == null)
                        return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                    if (file.AdderUserId != authenticate.UserId&&file.UserId!=authenticate.UserId)
                        return ServiceResultFactory.CreateError(false, MessageId.OnlyOwnerCanShare);
                    if (file.AdderUserId == authenticate.UserId)
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = authenticate.UserId, Access = new Access { Label = "", Value = Accessablity.Editor } });
                    }
                    else
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = file.AdderUserId.Value, Access = new Access { Label = "", Value = Accessablity.Editor } });
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = authenticate.UserId, Access = new Access { Label = "", Value = Accessablity.Editor } });
                    }

                    foreach (var user in model.Users)
                    {
                        result.Add(new FileDriveShare { Accessablity = user.Access.Value, FileId = model.EntityId, EntityType = EntityType.File, UserId = user.UserId });
                    }

                }

                await _shareRepository.AddRangeAsync(result);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException(false, ex);
            }
        }

        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetShareEntitiesAsync(AuthenticateDto authenticate)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                IOrderedQueryable<FileDriveDirectory> dbDirectoryQuery;
                IOrderedQueryable<FileDriveFile> dbFileQuery;
                List<FileDriveFilesListDto> fileList = new List<FileDriveFilesListDto>();
                List<FileDriverDirectoryListDto> directoryList = new List<FileDriverDirectoryListDto>();


                dbDirectoryQuery = _directoryRepository.Include(a => a.AdderUser)
                    .Where(a =>
                    !a.IsDeleted &&
                    a.ContractCode == authenticate.ContractCode &&
                    _shareRepository.Any(b => !b.IsDeleted && b.DirectoryId == a.DirectoryId && b.UserId == authenticate.UserId&&b.Status==ShareEntityStatus.Active) &&
                    !_shareRepository.Any(b => !b.IsDeleted && b.DirectoryId == a.ParentId && b.UserId == authenticate.UserId && b.Status == ShareEntityStatus.Active)

                    )
                    .OrderBy(a => a.CreatedDate);

                dbFileQuery = _fileRepository.Include(a => a.AdderUser).Include(a => a.Directory)
                    .Where(a =>
                    !a.IsDeleted &&
                    _shareRepository.Any(b => !b.IsDeleted && b.FileId == a.FileId && b.UserId == authenticate.UserId && b.Status == ShareEntityStatus.Active) &&
                    !_shareRepository.Any(b => !b.IsDeleted && b.DirectoryId == a.DirectoryId && b.UserId == authenticate.UserId && b.Status == ShareEntityStatus.Active) &&
                    a.Directory.ContractCode == authenticate.ContractCode &&
                    !a.Directory.IsDeleted

                    )
                    .OrderBy(a => a.CreatedDate);

                directoryList = await dbDirectoryQuery.Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate = a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = a.UpdateDate.ToPersianDateString(),
                    Id = a.DirectoryId,
                    Name = a.DirectoryName,
                    Size = _fileHelper.GetFileSizeSumFromDirectory(a.DirectoryPath).FormatSize(),
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserId = a.AdderUser.Id,
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image
                    }
                }).ToListAsync();
                fileList = await dbFileQuery.Select(a => new FileDriveFilesListDto
                {
                    CreateDate = a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = a.UpdateDate.ToPersianDateString(),
                    Id = a.FileId,
                    Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                    Extension = Path.GetExtension(a.FileName).Substring(1),
                    Size = a.FileSize.FormatSize(),
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserId = a.AdderUser.Id,
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image
                    }
                }).ToListAsync();
                FileDriveFileAndDirectoryListDto result = new FileDriveFileAndDirectoryListDto();
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetShareDirectoryInfoByIdAsync(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                

                if (!await _shareRepository.AnyAsync(a=>!a.IsDeleted&&a.UserId==authenticate.UserId&&a.DirectoryId==directoryId && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                    .AsNoTracking()
                    .Include(a => a.Directories)
                    .Include(a => a.Files)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId !=null);

                var directory = await dbQuery.FirstOrDefaultAsync();
                if (directory == null)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.EntityDoesNotExist);
                var directoryList = directory.Directories.Where(a => !a.IsDeleted&&_shareRepository.Any(b=>!b.IsDeleted&&b.DirectoryId==a.DirectoryId&&b.UserId==authenticate.UserId && b.Status == ShareEntityStatus.Active)).Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate = a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = a.UpdateDate.ToPersianDateString(),
                    Id = a.DirectoryId,
                    Name = a.DirectoryName,
                    Size = _fileHelper.GetFileSizeSumFromDirectory(a.DirectoryPath).FormatSize(),
                    UserAudit = _userRepository.Where(u => u.Id == a.AdderUserId).Select(b => new UserAuditLogDto
                    {
                        AdderUserId = b.Id,
                        AdderUserName = b.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + b.Image
                    }).FirstOrDefault()
                }).ToList();
                var fileList = directory.Files.Where(a => !a.IsDeleted && _shareRepository.Any(b => !b.IsDeleted && b.FileId == a.FileId && b.UserId == authenticate.UserId && b.Status == ShareEntityStatus.Active)).Select(a => new FileDriveFilesListDto
                {
                    CreateDate = a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = a.UpdateDate.ToPersianDateString(),
                    Id = a.FileId,
                    Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                    Extension = Path.GetExtension(a.FileName).Substring(1),
                    Size = a.FileSize.FormatSize(),
                    UserAudit = _userRepository.Where(u => u.Id == a.AdderUserId).Select(b => new UserAuditLogDto
                    {
                        AdderUserId = b.Id,
                        AdderUserName = b.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + b.Image
                    }).FirstOrDefault()
                }).ToList();
                FileDriveFileAndDirectoryListDto result = new FileDriveFileAndDirectoryListDto();
                var breadCrumb = await CreateBreadcrumbShare(directory.DirectoryPath);
                breadCrumb.Reverse();
                foreach (var item in breadCrumb)
                {
                    if (await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == item.DirectoryId && a.Status == ShareEntityStatus.Active))
                        result.Breadcrumbs.Add(item);
                    else
                        break;
                }
                result.Breadcrumbs.Reverse();
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriveSharedUserListDto>> GetShareForEntityByEntityIdAsync(AuthenticateDto authenticate, Guid entityId,EntityType entityType)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                {
                    if (entityType == EntityType.Directory && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == entityId && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError<FileDriveSharedUserListDto>(null, MessageId.AccessDenied);
                    if (entityType == EntityType.File && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == entityId && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                        return ServiceResultFactory.CreateError<FileDriveSharedUserListDto>(null, MessageId.AccessDenied);
                }
                var dbQuery =(entityType==EntityType.Directory)?await _shareRepository.Where(a => !a.IsDeleted && a.DirectoryId == entityId && a.Status == ShareEntityStatus.Active).OrderBy(a=>a.CreatedDate).ToListAsync():await _shareRepository.Where(a => !a.IsDeleted && a.FileId == entityId && a.Status == ShareEntityStatus.Active).OrderBy(a=>a.CreatedDate).ToListAsync();
                FileDriveSharedUserListDto result = new FileDriveSharedUserListDto();
                if (dbQuery != null&&dbQuery.Any())
                {
                    List<int> owners = new List<int>();
                    if (dbQuery.First().EntityType == EntityType.Directory)
                    {
                        owners = await FindOwnerOfDirectories(dbQuery.First().DirectoryId.Value, new List<int>());
                        var directory = await _directoryRepository.FirstOrDefaultAsync(a => a.DirectoryId == dbQuery.First().DirectoryId.Value);
                        owners.Add(directory.AdderUserId.Value);

                    }
                    else
                    {
                        var file = await _fileRepository.Include(a=>a.Directory).FirstOrDefaultAsync(a => a.FileId == entityId);
                        owners = await FindOwnerOfFiles(file.DirectoryId,new List<int>());
                        owners.Add(file.AdderUserId.Value);
                        owners.Add(file.Directory.AdderUserId.Value);
                    }
                    
                    

                    var usersResult =  dbQuery.Where(a=>!owners.Contains(a.UserId)).Select(a => new FileDriveSharedUserDto
                    {

                        Accessablity = a.Accessablity,
                        UserId = a.UserId,
                        FullName = _userRepository.First(b => b.Id == a.UserId).FullName,
                        UserName = _userRepository.First(b => b.Id == a.UserId).UserName,
                        Image = (!String.IsNullOrEmpty(_userRepository.First(b => b.Id == a.UserId).Image)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + _userRepository.First(b => b.Id == a.UserId).Image : null,

                    }).ToList();
                    var ownersResult = dbQuery.Where(a => owners.Contains(a.UserId)).Select(a => new FileDriveSharedUserDto
                    {

                        Accessablity = a.Accessablity,
                        UserId = a.UserId,
                        FullName = _userRepository.First(b => b.Id == a.UserId).FullName,
                        UserName = _userRepository.First(b => b.Id == a.UserId).UserName,
                        Image = (!String.IsNullOrEmpty(_userRepository.First(b => b.Id == a.UserId).Image)) ? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + _userRepository.First(b => b.Id == a.UserId).Image : null,

                    }).ToList();

                    foreach (var item in usersResult)
                        if (!result.Users.Any(a => a.UserId == item.UserId))
                            result.Users.Add(item);

                    foreach (var item in ownersResult)
                        if (!result.Owners.Any(a => a.UserId == item.UserId))
                            result.Owners.Add(item);
                }
                

                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<FileDriveSharedUserListDto>(null, ex);
            }
        }

        public async Task<ServiceResult<FileDriverDirectoryListDto>> CreateShareDirectoryAsync(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model)
        {
            try
            {

               

                if (!await _shareRepository.AnyAsync(a=>!a.IsDeleted&&a.DirectoryId==directoryId&&a.Accessablity==Accessablity.Editor&&a.UserId==authenticate.UserId))
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                   .AsNoTracking()
                   .Include(a => a.Directories)
                   .Include(a => a.ParentDirectory)
                   .Include(a => a.AdderUser)
                   .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId &&a.UserId!=null);


                var parentDirectory = await dbQuery.FirstOrDefaultAsync();
                if (parentDirectory == null)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.EntityDoesNotExist);
                if (parentDirectory.Directories.Any(b => !b.IsDeleted && b.DirectoryName == model.Name))
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.DuplicateDirectory);
                if(!_fileHelper.ValidateTitle(model.Name))
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.InvalidCharacter);
                FileDriveDirectory directory = new FileDriveDirectory();
                directory.DirectoryName = model.Name;
                directory.DirectoryPath = _fileHelper.CreateDirectory(parentDirectory.DirectoryPath, model.Name);
                directory.ContractCode = authenticate.ContractCode;
                directory.ParentId = parentDirectory.DirectoryId;
                directory.PermanentDelete = false;
                directory.UserId = parentDirectory.UserId;
                directory.Shares = new List<FileDriveShare>();
                var share = await _shareRepository.Where(a => !a.IsDeleted && a.DirectoryId == parentDirectory.DirectoryId && a.Status == ShareEntityStatus.Active).ToListAsync();

                foreach (var item in share)
                    directory.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity,Status=item.Status, EntityType = EntityType.Directory, UserId = item.UserId });

                await _directoryRepository.AddAsync(directory);
   
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                   
                   await _unitOfWork.SaveChangesAsync();
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = directory.UpdateDate.ToPersianDateString(),
                        Id = directory.DirectoryId,
                        Name = directory.DirectoryName,
                        Size = _fileHelper.GetFileSizeSumFromDirectory(directory.DirectoryPath).FormatSize(),
                        UserAudit = _userRepository.Where(a => a.Id == directory.AdderUserId).Select(a => new UserAuditLogDto
                        {
                            AdderUserId = a.Id,
                            AdderUserName = a.FullName,
                            AdderUserImage =(!String.IsNullOrEmpty(a.Image))? _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image:null
                        }).FirstOrDefault()
                    };

                    return ServiceResultFactory.CreateSuccess(result);
                }
                _fileHelper.RemoveDirectory(ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName), model.Name);
                return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriverDirectoryListDto>(null, exception);
            }
        }

        public async Task<DownloadFileDto> FileDriveShareDownloadFile(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {


                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == fileId && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return null;

                var dbQuery = await _fileRepository.Include(a => a.Directory).Where(a => !a.IsDeleted && a.FileId == fileId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;



                return await _fileHelper.DownloadFileDriveDocument(dbQuery.Directory.DirectoryPath, dbQuery.FileName);

            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<bool>> UpdateShareFileAsync(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model)
        {
            try
            {


                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == fileId && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId&&a.UserId!=null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (await dbQuery.AnyAsync(a => a.Directory.Files.Any(b =>!b.IsDeleted&& b.FileName == model.Name)))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateFile);
                var file = await dbQuery.FirstOrDefaultAsync();
                if(!_fileHelper.ValidateTitle(model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.InvalidCharacter);
                if (!_fileHelper.RenameFile(file.Directory.DirectoryPath, file.FileName, model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);

                file.FileName = model.Name + Path.GetExtension(file.FileName);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> UpdateShareDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model)
        {
            try
            {


                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == directoryId && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId&&a.UserId!=null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (await dbQuery.AnyAsync(a => a.ParentDirectory.Directories.Any(b => b.DirectoryName == model.Name)))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectory);
                if(!_fileHelper.ValidateTitle(model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.InvalidCharacter);
                var directory = await dbQuery.FirstOrDefaultAsync();

                directory.DirectoryPath = _fileHelper.RenameDirectory(ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName), directory.DirectoryPath, model.Name, directory.DirectoryName);
                directory.DirectoryName = model.Name;
                await ChangeAllSubDirectoryPathForUpdateDirectory(directory.DirectoryId, model.Name, directory.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<DownloadFileDto> DownloadShareFolderAsync(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {

                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == directoryId && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return null;

                var dbQuery = await _directoryRepository.Where(a => !a.IsDeleted && a.DirectoryId == directoryId && a.UserId != null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;

                return await _fileHelper.DownloadFileDriveShareFolder(dbQuery.DirectoryPath, _directoryRepository,_shareRepository,_fileRepository,authenticate.UserId);

            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadShareFolderAsync(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files)
        {
            try
            {
                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == directoryId && a.Accessablity == Accessablity.Editor && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null && a.UserId !=null).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId !=null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.EntityDoesNotExist);
                if (files == null || !files.Any())
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.FileNotFound);

                var parrentDirectory = files[0].Name.Substring(0, files[0].Name.IndexOf('/'));

                if (dbQuery.Directories.Any(a => !a.IsDeleted && a.DirectoryName == parrentDirectory))
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.DuplicateDirectory);

                foreach (var file in files)
                {

                    await _fileHelper.FileDriveSaveDocumentInUploadFolder(file, dbQuery.DirectoryPath, file.FileName);

                }

                var share = await _shareRepository.Where(a => !a.IsDeleted && a.DirectoryId == dbQuery.DirectoryId && a.Status == ShareEntityStatus.Active).ToListAsync();
                var directory = PrivateDirectoryUpload(_hostingEnvironmentRoot.ContentRootPath + dbQuery.DirectoryPath + parrentDirectory + "/", authenticate.ContractCode, dbQuery.UserId.Value, share);
                directory.ParentId = dbQuery.DirectoryId;
                directory.UserId = dbQuery.UserId;



                await _directoryRepository.AddAsync(directory);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = directory.UpdateDate.ToPersianDateString(),
                        Id = directory.DirectoryId,
                        Name = directory.DirectoryName,
                        Size = _fileHelper.GetFileSizeSumFromDirectory(directory.DirectoryPath).FormatSize(),
                        UserAudit = _userRepository.Where(a => a.Id == directory.AdderUserId).Select(a => new UserAuditLogDto
                        {
                            AdderUserId = a.Id,
                            AdderUserName = a.FullName,
                            AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.Image
                        }).FirstOrDefault()
                    };
                    return ServiceResultFactory.CreateSuccess(result);
                }
                return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.SaveFailed);

            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<FileDriverDirectoryListDto>(null, ex);
            }
        }

        public async Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadShareFileAsync(AuthenticateDto authenticate, Guid directoryId, IFormFile file)
        {
            try
            {

                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == directoryId && a.Accessablity == Accessablity.Editor && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Files).Include(a => a.Shares).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null && a.UserId !=null).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Files).Include(a=>a.Shares).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId !=null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.EntityDoesNotExist);
                if (file == null)
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.ModelStateInvalid);
                if (!file.IsDocumentExtentionValid())
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.InvalidFileExtention);

                if (!file.IsDocumentSizeValid())
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.FileSizeError);
                if(!_fileHelper.ValidateTitle(file.FileName))
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.InvalidCharacter);
                string fileName = file.FileName;
                int counter = 1;
                while (true)
                {
                    if (dbQuery.Files.Any(a => !a.IsDeleted && a.FileName == fileName))
                    {
                        fileName = file.FileName.Substring(0, file.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(file.FileName);
                        counter++;
                    }
                    else
                        break;
                }


                List<FileDriveShare> shares = new List<FileDriveShare>();
                if (dbQuery.Shares != null && dbQuery.Shares.Any(a => !a.IsDeleted&& a.Status == ShareEntityStatus.Active))
                    foreach (var item in dbQuery.Shares.Where(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active))
                        shares.Add(new FileDriveShare { EntityType = EntityType.File, UserId = item.UserId, Accessablity = item.Accessablity });
                var saveResult = await _fileHelper.FileDriveSaveDocument(file, dbQuery.DirectoryPath, fileName);

                if (dbQuery.Files != null && dbQuery.Files.Any())
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length, UserId = dbQuery.UserId, Shares = shares });
                else
                {
                    dbQuery.Files = new List<FileDriveFile>();
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length, UserId = dbQuery.UserId, Shares = shares });
                }
                if (saveResult)
                {
                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {
                        var result = dbQuery.Files.OrderByDescending(a => a.CreatedDate).Select(a => new FileDriveFilesListDto
                        {
                            CreateDate = a.CreatedDate.ToPersianDateString(),
                            Extension = Path.GetExtension(a.FileName).Substring(1),
                            Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                            Id = a.FileId,
                            ModifiedDate = a.UpdateDate.ToPersianDateString(),
                            Size = a.FileSize.FormatSize(),
                            UserAudit = _userRepository.Where(a => a.Id == authenticate.UserId).Select(b => new UserAuditLogDto
                            {
                                AdderUserId = b.Id,
                                AdderUserName = b.FullName,
                                AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + b.Image
                            }).FirstOrDefault()
                        }).FirstOrDefault();
                        return ServiceResultFactory.CreateSuccess(result);
                    }
                    _fileHelper.FileDriveRemoveDocument(dbQuery.DirectoryPath, fileName);
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.SaveFailed);
                }

                return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.UploudFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFilesListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteShareDirectoryAsync(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.DirectoryId == directoryId && a.Accessablity == Accessablity.Editor && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId !=null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var directory = await dbQuery.FirstOrDefaultAsync();
                if(directory.AdderUserId!=authenticate.UserId&&directory.UserId!=authenticate.UserId)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                var shareEntityStatus = (directory.UserId == authenticate.UserId) ? ShareEntityStatus.IsOwnerTrash : ShareEntityStatus.IsEditorTrash;
                await DeleteAllSubFileAndDirectoryRecord(directory.DirectoryId, shareEntityStatus);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.MoveDirectoryToTrash(directory.DirectoryPath, directory.DirectoryId.ToString(), authenticate.ContractCode);


                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteShareFileAsync(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                if (!await _shareRepository.AnyAsync(a => !a.IsDeleted && a.FileId == fileId && a.Accessablity == Accessablity.Editor && a.UserId == authenticate.UserId && a.Status == ShareEntityStatus.Active))
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .Include(a => a.AdderUser)
                .Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && a.UserId !=null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var file = await dbQuery.FirstOrDefaultAsync();
                if (file.AdderUserId != authenticate.UserId && file.UserId != authenticate.UserId)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                var shareEntityStatus = (file.UserId == authenticate.UserId) ? ShareEntityStatus.IsOwnerTrash : ShareEntityStatus.IsEditorTrash;
                file.IsDeleted = true;
                foreach (var item in file.Shares)
                    item.Status = shareEntityStatus;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.MoveFileToTrash(file.Directory.DirectoryPath + file.FileName, file.FileId.ToString() + Path.GetExtension(file.FileName), authenticate.ContractCode);
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        private async Task DeleteAllSubFileAndDirectoryRecord(Guid directoryId, ShareEntityStatus status)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares).Include(a => a.Files).FirstAsync(a => a.DirectoryId == directoryId);
                directory.IsDeleted = true;
                foreach (var item in directory.Shares)
                    item.Status = status;
                foreach (var file in directory.Files)
                {
                    var shares = await _shareRepository.Where(a => a.FileId == file.FileId).ToListAsync();
                    foreach (var item in shares)
                        item.Status = status;
                    file.IsDeleted = true;
                }
                    
                foreach (var dir in directory.Directories)
                    await DeleteAllSubFileAndDirectoryRecord(dir.DirectoryId,status);

            }
            catch (Exception ex)
            {

            }
        }
        private async Task ChangeAllSubDirectoryPathForUpdateDirectory(Guid directoryId, string newName, int index)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).FirstAsync(a => a.DirectoryId == directoryId);
                var paths = directory.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                paths[index] = newName;
                directory.DirectoryPath = "/" + String.Join('/', paths) + "/";
                foreach (var dir in directory.Directories)
                    await ChangeAllSubDirectoryPathForUpdateDirectory(dir.DirectoryId, newName, index);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task AddSharingRecordForAllSubDirectory(Guid directoryId, List<FileDriveShareUserAccessablityDto> users)
        {

            try
            {
                
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Include(a=>a.Shares).FirstAsync(a => a.DirectoryId == directoryId);
                foreach (var user in users)
                {
                    if (directory.Shares != null)
                        directory.Shares.Add(new FileDriveShare { Accessablity = user.Access.Value, Status = ShareEntityStatus.Active, EntityType = EntityType.Directory, UserId = user.UserId });
                    else
                        directory.Shares = new List<FileDriveShare> { new FileDriveShare { Accessablity = user.Access.Value, Status = ShareEntityStatus.Active, EntityType = EntityType.Directory, UserId = user.UserId } };
                    foreach (var file in directory.Files)
                    {
                        var files = await _fileRepository.Include(a => a.Shares).FirstAsync(a => a.FileId == file.FileId);
                        if (files.Shares != null)
                            files.Shares.Add(new FileDriveShare { Accessablity = user.Access.Value,Status=ShareEntityStatus.Active, EntityType = EntityType.File, UserId = user.UserId });
                        else
                            files.Shares = new List<FileDriveShare> { new FileDriveShare { Accessablity = user.Access.Value, Status = ShareEntityStatus.Active, EntityType = EntityType.File, UserId = user.UserId } };
                    }
                }

                foreach (var dir in directory.Directories)
                    await AddSharingRecordForAllSubDirectory(dir.DirectoryId, users);

            }
            catch (Exception ex)
            {

            }
        }
        private async Task RemoveSharingRecordForAllSubDirectory(Guid directoryId, List<FileDriveShareUserAccessablityDto> users)
        {

            try
            {

                var directory = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Include(a => a.Shares).FirstAsync(a => a.DirectoryId == directoryId);
                foreach (var user in users)
                {
                    foreach (var item in directory.Shares.Where(a=>a.UserId==user.UserId && a.Status == ShareEntityStatus.Active))
                        item.IsDeleted = true;
                   
                    foreach (var file in directory.Files)
                    {
                        var files = await _fileRepository.Include(a => a.Shares).FirstAsync(a => a.FileId == file.FileId);
                        foreach (var item in files.Shares.Where(a => a.UserId == user.UserId && a.Status == ShareEntityStatus.Active))
                        item.IsDeleted = true;
                    }
                }

                foreach (var dir in directory.Directories)
                    await RemoveSharingRecordForAllSubDirectory(dir.DirectoryId, users);

            }
            catch (Exception ex)
            {

            }
        }
        //private  IEnumerable<FileDriveShare> CopyAllSubDirectoryPathRecordForMoveDirectory(List<FileDriveDirectory> directoryModel,List<FileDriveShare> result, Guid directoryId, Accessablity accessablity, int userId)
        //{



        //    var directory =  directoryModel.First(a => a.DirectoryId == directoryId);

        //    if (directory.Directories != null && directory.Directories.Any())
        //    {
        //        foreach (var dir in directoryModel.Where(a => a.ParentId == directoryId))
        //        {
        //            var paths = dir.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        //             new FileDriveShare
        //            {
        //                Accessablity = accessablity,
        //                DirectoryId = dir.DirectoryId,
        //                EntityType = EntityType.Directory,
        //                UserId = userId
        //            };
        //            foreach (var item in dir.Files)
        //            {
        //                 new FileDriveShare
        //                {
        //                    Accessablity = accessablity,
        //                    FileId = item.FileId,
        //                    EntityType = EntityType.File,
        //                    UserId = userId
        //                };
        //            }
        //           return  CopyAllSubDirectoryPathRecordForMoveDirectory(directoryModel,result, dir.DirectoryId, accessablity, userId);
        //        }
        //    }
        //    if ((directory.Directories == null || !directory.Directories.Any()))
        //    {
        //         new FileDriveShare
        //        {
        //            Accessablity = accessablity,
        //            DirectoryId = directory.DirectoryId,
        //            EntityType = EntityType.Directory,
        //            UserId = userId
        //        };
        //        if (directory.Files != null && directory.Files.Any())
        //        {
        //            foreach (var file in directory.Files)
        //            {
        //                 new FileDriveShare
        //                {
        //                    Accessablity = accessablity,
        //                    FileId = file.FileId,
        //                    EntityType = EntityType.File,
        //                    UserId = userId
        //                };
        //            }
        //        }

        //    }

        //}
        private async Task<List<FileDriveBreadcrumbDto>> CreateBreadcrumbShare(string directoryPath)
        {
            var result = new List<FileDriveBreadcrumbDto>();
            var directories = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (directories.Length > 5)
            {
                var fixedDir = "/" + directories[0] + "/" + directories[1] + "/" + directories[2] + "/" + directories[3] + "/" + directories[4] + "/";
                for (int i = 5; i < directories.Length; i++)
                {
                    var temp = new FileDriveBreadcrumbDto();
                    var dir = await _directoryRepository.FirstOrDefaultAsync(a => a.DirectoryPath == (fixedDir + directories[i] + "/"));
                    temp.DirectoryName = directories[i];
                    temp.DirectoryId = dir.DirectoryId;
                    fixedDir += dir.DirectoryName + "/";
                    result.Add(temp);
                }
            }
            return result;
        }
        private async Task<List<int>> FindOwnerOfDirectories(Guid directoryId,List<int> result)
        {

            var directory = await _directoryRepository.Include(a=>a.ParentDirectory).FirstOrDefaultAsync(a => a.DirectoryId == directoryId);
            if(directory.ParentDirectory!=null &&await _shareRepository.AnyAsync(a=>!a.IsDeleted && a.Status == ShareEntityStatus.Active && a.DirectoryId ==directory.ParentId))
            {
                await FindOwnerOfDirectories(directory.ParentId.Value, result);
                result.Add(directory.ParentDirectory.AdderUserId.Value);

            }
            else
            {
                result.Add(directory.AdderUserId.Value);
            }
            return result;
        }
        private async Task<List<int>> FindOwnerOfFiles(Guid directoryId,List<int> result)
        {
            var directory = await _fileRepository.Include(a => a.Directory).FirstOrDefaultAsync(a => a.DirectoryId == directoryId);
            if (directory.Directory != null && await _shareRepository.AnyAsync(a =>!a.IsDeleted && a.Status == ShareEntityStatus.Active&& a.DirectoryId == directory.DirectoryId))
            if (directory.Directory != null && await _shareRepository.AnyAsync(a =>!a.IsDeleted && a.Status == ShareEntityStatus.Active&& a.DirectoryId == directory.DirectoryId))
            {
                await FindOwnerOfDirectories(directory.DirectoryId, result);
                result.Add(directory.Directory.AdderUserId.Value);

            }
            else
            {
                result.Add(directory.AdderUserId.Value);
            }
            return result;
        }

        

        private FileDriveDirectory PrivateDirectoryUpload(string sourceDirName, string contractCode, int userId, List<FileDriveShare> share)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            FileDriveDirectory directory = new FileDriveDirectory();
            var finalPath = (dir.FullName.Replace('\\', '/').LastIndexOf('/') == dir.FullName.Length - 1) ? "" : "/";
            directory.ContractCode = contractCode;
            directory.DirectoryName = dir.Name;
            directory.DirectoryPath = dir.FullName.Substring(dir.FullName.IndexOf("\\Files\\")) + finalPath;
            directory.DirectoryPath = directory.DirectoryPath.Replace("\\", "/");
            directory.UserId = userId;
            FileInfo[] files = dir.GetFiles();
            directory.Directories = new List<FileDriveDirectory>();
            directory.Files = new List<FileDriveFile>();
            directory.Shares = new List<FileDriveShare>();
            if (share != null && share.Any())
            {
                foreach (var item in share)
                    directory.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, EntityType = EntityType.Directory, UserId = item.UserId, IsDeleted = item.IsDeleted });
            }
            foreach (FileInfo file in files)
            {

                directory.Files.Add(new FileDriveFile { FileName = file.Name, FileSize = file.Length, UserId = userId });
            }
            if (directory.Files != null && directory.Files.Any())
            {
                foreach (var file in directory.Files)
                    if (share != null && share.Any())
                    {
                        file.Shares = new List<FileDriveShare>();
                        foreach (var item in share)
                        {
                           
                            file.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, EntityType = EntityType.File, UserId = item.UserId, IsDeleted = item.IsDeleted });
                        }
                       


                    }
            }
            DirectoryInfo[] dirs = dir.GetDirectories();


            foreach (DirectoryInfo subdir in dirs)
            {

                directory.Directories.Add(PrivateDirectoryUpload(subdir.FullName, contractCode, userId, share));
            }
            return directory;
        }

    }
}
