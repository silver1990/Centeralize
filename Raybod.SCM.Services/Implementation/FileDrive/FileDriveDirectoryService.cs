using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
using Raybod.SCM.Utility.Utility.TreeModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class FileDriveDirectoryService : IFileDriveDirectoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<FileDriveDirectory> _directoryRepository;
        private readonly DbSet<FileDriveFile> _fileRepository;
        private readonly DbSet<User> _userRepository;
        private readonly DbSet<FileDriveShare> _shareRepository;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly CompanyConfig _appSettings;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;
        public FileDriveDirectoryService(IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor
          , IWebHostEnvironment hostingEnvironmentRoot)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _directoryRepository = _unitOfWork.Set<FileDriveDirectory>();
            _fileRepository = _unitOfWork.Set<FileDriveFile>();
            _shareRepository = _unitOfWork.Set<FileDriveShare>();
            _userRepository = _unitOfWork.Set<User>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }


        #region Public
        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetDirectoryInfoById(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                    .AsNoTracking()
                    .Include(a => a.Directories)
                    .Include(a => a.Files)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null);

                var directory = await dbQuery.FirstOrDefaultAsync();
                if (directory == null)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.EntityDoesNotExist);
                var directoryList = directory.Directories.Where(a => !a.IsDeleted).Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate =(authenticate.language=="en")?a.CreatedDate.Value.ToString("yyyy/MM/dd"): a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                var fileList = directory.Files.Where(a => !a.IsDeleted).Select(a => new FileDriveFilesListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                result.Breadcrumbs = await CreateBreadcrumb(directory.DirectoryPath);
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetRootDirectoryInfo(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                    .AsNoTracking()
                    .Include(a => a.Directories)
                    .Include(a => a.Files)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryPath == ServiceSetting.FileDriveRootPath(authenticate.ContractCode) && a.UserId == null);

                if (await dbQuery.CountAsync() == 0)
                {
                    FileDriveDirectory root = new FileDriveDirectory();
                    root.DirectoryName = "FileDrive";
                    root.DirectoryPath = _fileHelper.CreateRootDirectory(ServiceSetting.FileDriveRootPath(authenticate.ContractCode));
                    root.ContractCode = authenticate.ContractCode;
                    root.PermanentDelete = false;
                    await _directoryRepository.AddAsync(root);

                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {

                        return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(new FileDriveFileAndDirectoryListDto());
                    }
                    _fileHelper.RemoveRootDirectory(ServiceSetting.FileDriveRootPath(authenticate.ContractCode));
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.SaveFailed);
                }
                var targetDirectory = await dbQuery.FirstOrDefaultAsync();
                var directoryList = targetDirectory.Directories.Where(a => !a.IsDeleted).Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                var fileList = targetDirectory.Files.Where(a => !a.IsDeleted).Select(a => new FileDriveFilesListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriverDirectoryListDto>> CreateDirectory(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model)
        {
            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);
                IQueryable<FileDriveDirectory> dbQuery;
                if (directoryId != null)
                {
                    dbQuery = _directoryRepository
                   .AsNoTracking()
                   .Include(a => a.Directories)
                   .Include(a => a.ParentDirectory)
                   .Include(a => a.AdderUser)
                   .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null);
                }
                else
                {
                    dbQuery = _directoryRepository
               .AsNoTracking()
               .Include(a => a.Directories)
               .Include(a => a.ParentDirectory)
               .Include(a => a.AdderUser)
               .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryPath == ServiceSetting.FileDriveRootPath(authenticate.ContractCode));
                }


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
                await _directoryRepository.AddAsync(directory);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = (authenticate.language == "en") ? directory.CreatedDate.Value.ToString("yyyy/MM/dd") : directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = (authenticate.language == "en") ? directory.UpdateDate.Value.ToString("yyyy/MM/dd") : directory.UpdateDate.ToPersianDateString(),
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
                _fileHelper.RemoveDirectory(ServiceSetting.FileDriveRootPath(authenticate.ContractCode), model.Name);
                return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriverDirectoryListDto>(null, exception);
            }
        }


        public async Task<ServiceResult<bool>> UpdateDirectory(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (await dbQuery.AnyAsync(a => a.ParentDirectory.Directories.Any(b =>!b.IsDeleted&& b.DirectoryName == model.Name)))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectory);
                if(!_fileHelper.ValidateTitle(model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.InvalidCharacter);
                var directory = await dbQuery.FirstOrDefaultAsync();

                directory.DirectoryPath = _fileHelper.RenameDirectory(ServiceSetting.FileDriveRootPath(authenticate.ContractCode), directory.DirectoryPath, model.Name, directory.DirectoryName);
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

        public async Task<ServiceResult<bool>> DeleteDirectory(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var directory = await dbQuery.FirstOrDefaultAsync();

                await DeleteAllSubFileAndDirectoryRecord(directory.DirectoryId);
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
        public async Task<ServiceResult<bool>> DeleteDirectoryPermanently(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                IQueryable<FileDriveDirectory> dbQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPrivatePermission&&!permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == authenticate.UserId || _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if(!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == null || _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == null || a.UserId == authenticate.UserId || _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else 
                {
                    dbQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && ( _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }




                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var directory = await dbQuery.FirstOrDefaultAsync();

                await DeletePermanentAllSubFileAndDirectoryRecord(directory.DirectoryId);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.DeleteDirectoryFromPath(ServiceSetting.FileDriveTrashPath(authenticate.ContractCode) + (directory.DirectoryId.ToString()));
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> DeleteAllEntityPermanently(AuthenticateDto authenticate)
        {
            try
            {
                IQueryable<FileDriveDirectory> dbDirectoryQuery;
                IQueryable<FileDriveFile> dbFileQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPrivatePermission && !permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Include(a => a.Shares)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId || _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId || _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Include(a => a.Shares)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == null || _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == null || _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Include(a => a.Shares)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId || a.UserId == null || _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId || a.UserId == null || _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Include(a => a.Shares)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (_shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (_shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }


                var directories = await dbDirectoryQuery.ToListAsync();
                var parentDirectories = directories.Where(a => !a.ParentDirectory.IsDeleted).ToList();
                foreach (var directory in directories)
                {
                    foreach(var item in directory.Shares)
                    {
                        item.IsDeleted = true;
                        item.Status = ShareEntityStatus.IsOwnerTrash;
                    }
                    directory.IsDeleted = true;
                    directory.PermanentDelete = true;
                }






                var files = await dbFileQuery.ToListAsync();
                var singleFiles = files.Where(a => !a.Directory.IsDeleted);



                foreach (var file in files)
                {
                    foreach (var item in file.Shares)
                    {
                        item.IsDeleted = true;
                        item.Status = ShareEntityStatus.IsOwnerTrash;
                    }
                    file.IsDeleted = true;
                    file.PermanentDelete = true;
                }

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    foreach (var directory in parentDirectories)
                        _fileHelper.DeleteDirectoryFromPath(ServiceSetting.FileDriveTrashPath(authenticate.ContractCode) + (directory.DirectoryId.ToString()));
                    foreach (var file in singleFiles)
                        _fileHelper.DeleteDocumentFromPath(ServiceSetting.FileDriveTrashPath(authenticate.ContractCode) + (file.FileId.ToString() + Path.GetExtension(file.FileName)));
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> RestoreDirectory(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                IQueryable<FileDriveDirectory> dbQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPrivatePermission && !permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
                    .Include(a => a.Directories)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == authenticate.UserId||_shareRepository.Any(b=>b.DirectoryId==directoryId&&b.UserId==a.AdderUserId&&(b.Status==ShareEntityStatus.IsEditorTrash||b.Status==ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
               .Include(a => a.Directories)
               .Include(a => a.ParentDirectory)
               .Include(a => a.AdderUser)
               .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == null || a.UserId == authenticate.UserId || _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }

                else if (!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _directoryRepository
               .Include(a => a.Directories)
               .Include(a => a.ParentDirectory)
               .Include(a => a.AdderUser)
               .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId == null || _shareRepository.Any(b => b.DirectoryId == directoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else
                {
                    dbQuery = _directoryRepository
                   .Include(a => a.Directories)
                   .Include(a => a.ParentDirectory)
                   .Include(a => a.AdderUser)
                   .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && (a.UserId!=null&&_shareRepository.Any(b=>b.DirectoryId==directoryId&&b.AdderUserId==authenticate.UserId&&(b.Status==ShareEntityStatus.IsEditorTrash||b.Status==ShareEntityStatus.IsOwnerTrash))));
                }




                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var directory = await dbQuery.FirstOrDefaultAsync();
                if (_fileHelper.IsDirectoryExist(directory.DirectoryPath))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectory);
                await RestoreAllSubFileAndDirectoryRecord(directory.DirectoryId);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.RestoreDirectoryFromTrash(directory.DirectoryPath, directory.DirectoryId.ToString(), authenticate.ContractCode);

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> RestoreAllEntities(AuthenticateDto authenticate)
        {
            try
            {
                IQueryable<FileDriveDirectory> dbDirectoryQuery;
                IQueryable<FileDriveFile> dbFileQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPrivatePermission&&!permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId||_shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId|| _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == null || _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == null || _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId||a.UserId==null || _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a => a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && (a.UserId == authenticate.UserId||a.UserId==null || _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else
                {
                    dbDirectoryQuery = _directoryRepository
              .Include(a => a.Directories)
              .Include(a => a.ParentDirectory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && ( _shareRepository.Any(b => b.DirectoryId == a.DirectoryId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));

                    dbFileQuery = _fileRepository.Include(a => a.Directory).Include(a=>a.Shares)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && ( _shareRepository.Any(b => b.FileId == a.FileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }



                var directories = await dbDirectoryQuery.ToListAsync();
                var parentDirectories = directories.Where(a => !a.ParentDirectory.IsDeleted).ToList();


                var files = await dbFileQuery.ToListAsync();
                var singleFiles = files.Where(a => !a.Directory.IsDeleted);

                foreach (var directory in parentDirectories)
                {
                    if (_fileHelper.RestoreDirectoryFromTrash(directory.DirectoryPath, directory.DirectoryId.ToString(), authenticate.ContractCode))
                    {
                        await RestoreAllSubFileAndDirectoryRecord(directory.DirectoryId);
                    }
                }


                foreach (var file in singleFiles)
                {
                    if (_fileHelper.RestoreFileFromTrash(file.Directory.DirectoryPath + file.FileName, file.FileId.ToString() + Path.GetExtension(file.FileName), authenticate.ContractCode))
                    {
                        foreach(var item in file.Shares)
                        {
                            item.Status = ShareEntityStatus.Active;
                        }
                        file.IsDeleted = false;
                    }
                }



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
        public async Task<ServiceResult<List<ExpandoObject>>> GetDirectoryTreeAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ExpandoObject>>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.UserId == null).OrderBy(a => a.CreatedDate);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<List<ExpandoObject>>(null, MessageId.EntityDoesNotExist);
                var directories = await dbQuery.Select(a => new ObjectTreeDto
                {
                    Value = a.DirectoryId,
                    ParentId = a.ParentId.Value,
                    Label = a.DirectoryName
                }).ToListAsync();

                directories.First(a => a.ParentId == Guid.Empty).Label = "Root";
                var treeView = CreateTreeModelOfDirectories(directories, Guid.Empty).ToList();
                var stringtreeView = JsonConvert.SerializeObject(treeView);
                var result = JsonConvert.DeserializeObject<List<ExpandoObject>>(stringtreeView, new ExpandoObjectConverter());
                return ServiceResultFactory.CreateSuccess<List<ExpandoObject>>(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ExpandoObject>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<AdvanceSearchDto>>> GetAdvanceSearchDataAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<AdvanceSearchDto>>(null, MessageId.AccessDenied);

                var dbDirectoryQuery = _directoryRepository
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.UserId == null).OrderBy(a => a.CreatedDate);

                var dbFileQuery = _fileRepository.Include(a => a.Directory)
                .Where(a => !a.IsDeleted && !a.Directory.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.UserId == null).OrderBy(a => a.CreatedDate);


                var directories = await dbDirectoryQuery.Select(a => new AdvanceSearchDto
                {
                    Id = a.DirectoryId,
                    Name = a.DirectoryName,
                    IsFile = false
                }).ToListAsync();

                var files = await dbFileQuery.Select(a => new AdvanceSearchDto
                {
                    Id = a.FileId,
                    Name = a.FileName,
                    IsFile = true,
                }).ToListAsync();

                var result = new List<AdvanceSearchDto>();
                result.AddRange(directories);
                result.AddRange(files);
                return ServiceResultFactory.CreateSuccess<List<AdvanceSearchDto>>(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<AdvanceSearchDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> MoveDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (directoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId && a.UserId == null).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                if (dbDistinationQuery.Directories.Any(a => !a.IsDeleted && a.DirectoryName == dbQuery.DirectoryName))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectoryInDestination);
                var oldPath = dbQuery.DirectoryPath;

                var newPath = _fileHelper.MoveDirectory(dbQuery.DirectoryPath, dbDistinationQuery.DirectoryPath, dbQuery.DirectoryName);
                int index = dbQuery.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1;
                await ChangeAllSubDirectoryPathRecordForMoveDirectory(dbQuery.DirectoryId, newPath, index,new List<FileDriveShare>());
                dbQuery.ParentId = dbDistinationQuery.DirectoryId;


                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                _fileHelper.MoveDirectory(oldPath, newPath);
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> CopyDirectoryAsync(AuthenticateDto authenticate, Guid directoryId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                if (directoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId && a.UserId == null).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                if (dbDistinationQuery.Directories.Any(a => !a.IsDeleted && a.DirectoryName == dbQuery.DirectoryName))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectoryInDestination);
                var oldPath = dbQuery.DirectoryPath;

                var newPath = _fileHelper.CopyDirectory(dbQuery.DirectoryPath, dbDistinationQuery.DirectoryPath, dbQuery.DirectoryName);
                int index = dbQuery.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1;
                var directories = await _directoryRepository.Include(a => a.Files).Where(a => !a.IsDeleted && !a.PermanentDelete).ToListAsync();
                var result = CopyAllSubDirectoryPathRecordForMoveDirectory(directories, dbQuery.DirectoryId, dbQuery.ParentId, newPath, index, 0,new List<FileDriveShare>(),new List<FileDriveShare>()).ToList();
                result[0].ParentId = destinationId;
                await _directoryRepository.AddAsync(result[0]);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                _fileHelper.MoveDirectory(oldPath, newPath);
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<DownloadFileDto> DownloadFolderAsync(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = await _directoryRepository.Where(a => !a.IsDeleted && a.DirectoryId == directoryId && a.UserId == null).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;

                return await _fileHelper.DownloadFileDriveFolder(dbQuery.DirectoryPath);

            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<FileDriveTrashContentDto>> GetTrashContentAsync(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                IOrderedQueryable<FileDriveDirectory> dbDirectoryQuery;
                IOrderedQueryable<FileDriveFile> dbFileQuery;
                List<FileDriveFileTrashContentDto> fileList = new List<FileDriveFileTrashContentDto>();
                List<FileDriveDirectoryTrashContentDto> directoryList = new List<FileDriveDirectoryTrashContentDto>();
                if (permission.HasPrivatePermission && !permission.HasPublicPermission)
                {

                    dbDirectoryQuery = _directoryRepository.Include(a => a.AdderUser)
                   .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && !a.ParentDirectory.IsDeleted && (a.UserId == authenticate.UserId || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash) && b.DirectoryId == a.DirectoryId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);
                    dbFileQuery = _fileRepository.Include(a => a.AdderUser).Include(a => a.Directory)
                    .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && !a.Directory.IsDeleted && (a.UserId == authenticate.UserId || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash) && b.FileId == a.FileId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);

                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository.Include(a => a.AdderUser)
                        .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && !a.ParentDirectory.IsDeleted && (a.UserId == null || a.UserId == authenticate.UserId || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash) && b.DirectoryId == a.DirectoryId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);

                    dbFileQuery = _fileRepository.Include(a => a.AdderUser).Include(a => a.Directory)
                        .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && !a.Directory.IsDeleted && (a.UserId == null || a.UserId == authenticate.UserId || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash) && b.FileId == a.FileId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);

                }
                else if (!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbDirectoryQuery = _directoryRepository.Include(a => a.AdderUser)
                        .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && !a.ParentDirectory.IsDeleted && (a.UserId == null || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash) && b.DirectoryId == a.DirectoryId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);

                    dbFileQuery = _fileRepository.Include(a => a.AdderUser).Include(a => a.Directory)
                        .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && !a.Directory.IsDeleted && (a.UserId == null || _shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash) && b.FileId == a.FileId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);
                }
                else
                {
                    dbDirectoryQuery = _directoryRepository.Include(a => a.AdderUser)
                       .Where(a => a.IsDeleted && !a.PermanentDelete && a.ContractCode == authenticate.ContractCode && !a.ParentDirectory.IsDeleted && (_shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash) && b.DirectoryId == a.DirectoryId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);

                    dbFileQuery = _fileRepository.Include(a => a.AdderUser).Include(a => a.Directory)
                        .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && !a.Directory.IsDeleted && (_shareRepository.Any(b => (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash) && b.FileId == a.FileId && a.AdderUserId == authenticate.UserId))).OrderBy(a => a.CreatedDate);
                }
                directoryList = await dbDirectoryQuery.Select(a => new FileDriveDirectoryTrashContentDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
                    Id = a.DirectoryId,
                    Name = a.DirectoryName,
                    Size = _fileHelper.GetFileSizeSumFromDirectory(ServiceSetting.FileDriveTrashPath(authenticate.ContractCode) + (a.DirectoryId.ToString())).FormatSize(),
                    Path = (a.UserId != null) ? a.DirectoryPath.CreatePathPrivate() : a.DirectoryPath.CreatePath(),
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserId = a.AdderUser.Id,
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image
                    }
                }).ToListAsync();
                fileList = await dbFileQuery.Select(a => new FileDriveFileTrashContentDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
                    Id = a.FileId,
                    Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                    Extension = Path.GetExtension(a.FileName).Substring(1),
                    Size = a.FileSize.FormatSize(),
                    Path = (a.UserId != null) ? a.Directory.DirectoryPath.CreatePathPrivate() : a.Directory.DirectoryPath.CreatePath() + "/" + a.FileName,
                    UserAudit = new UserAuditLogDto
                    {
                        AdderUserId = a.AdderUser.Id,
                        AdderUserName = a.AdderUser.FullName,
                        AdderUserImage = _appSettings.WepApiHost + ServiceSetting.UploadImagesPath.UserSmall + a.AdderUser.Image
                    }
                }).ToListAsync();
                FileDriveTrashContentDto result = new FileDriveTrashContentDto();
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveTrashContentDto>(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveTrashContentDto>(null, exception);
            }
        }
        public async Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadFolder(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null && a.UserId == null).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == null).FirstOrDefaultAsync();

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


                var directory = DirectoryUpload(_hostingEnvironmentRoot.ContentRootPath + dbQuery.DirectoryPath + parrentDirectory + "/", authenticate.ContractCode);
                directory.ParentId = dbQuery.DirectoryId;

                await _directoryRepository.AddAsync(directory);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = (authenticate.language == "en") ? directory.CreatedDate.Value.ToString("yyyy/MM/dd") : directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = (authenticate.language == "en") ? directory.UpdateDate.Value.ToString("yyyy/MM/dd") : directory.UpdateDate.ToPersianDateString(),
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
        #endregion

        #region private

        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetDirectoryInfoByIdPrivatly(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                    .AsNoTracking()
                    .Include(a => a.Directories)
                    .Include(a => a.Files)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId);

                var directory = await dbQuery.FirstOrDefaultAsync();
                if (directory == null)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.EntityDoesNotExist);
                var directoryList = directory.Directories.Where(a => !a.IsDeleted).Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                var fileList = directory.Files.Where(a => !a.IsDeleted).Select(a => new FileDriveFilesListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                result.Breadcrumbs = await CreateBreadcrumbPrivate(directory.DirectoryPath);
                result.Directories = directoryList;
                result.Files = fileList;
                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriveFileAndDirectoryListDto>> GetRootDirectoryInfoPrivatly(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                    .AsNoTracking()
                    .Include(a => a.Directories)
                    .Include(a => a.Files)
                    .Include(a => a.ParentDirectory)
                    .Include(a => a.AdderUser)
                    .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryPath == ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName) && a.UserId == authenticate.UserId);

                if (await dbQuery.CountAsync() == 0)
                {
                    FileDriveDirectory root = new FileDriveDirectory();
                    root.DirectoryName = authenticate.UserName;
                    root.DirectoryPath = _fileHelper.CreateRootDirectory(ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName));
                    root.ContractCode = authenticate.ContractCode;
                    root.PermanentDelete = false;
                    root.UserId = authenticate.UserId;
                    await _directoryRepository.AddAsync(root);

                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {

                        return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(new FileDriveFileAndDirectoryListDto());
                    }
                    _fileHelper.RemoveRootDirectory(ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName));
                    return ServiceResultFactory.CreateError<FileDriveFileAndDirectoryListDto>(null, MessageId.SaveFailed);
                }
                var targetDirectory = await dbQuery.FirstOrDefaultAsync();
                var directoryList = targetDirectory.Directories.Where(a => !a.IsDeleted).Select(a => new FileDriverDirectoryListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                var fileList = targetDirectory.Files.Where(a => !a.IsDeleted).Select(a => new FileDriveFilesListDto
                {
                    CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                    ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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
                result.Directories = directoryList;
                result.Files = fileList;

                return ServiceResultFactory.CreateSuccess<FileDriveFileAndDirectoryListDto>(result);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriveFileAndDirectoryListDto>(null, exception);
            }
        }

        public async Task<ServiceResult<FileDriverDirectoryListDto>> CreateDirectoryPrivatly(AuthenticateDto authenticate, Guid? directoryId, FileDriveDirectoryCreateDto model)
        {
            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);
                IQueryable<FileDriveDirectory> dbQuery;
                if (directoryId != null)
                {
                    dbQuery = _directoryRepository
                   .AsNoTracking()
                   .Include(a => a.Directories)
                   .Include(a => a.ParentDirectory)
                   .Include(a => a.AdderUser)
                   .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId);
                }
                else
                {
                    dbQuery = _directoryRepository
               .AsNoTracking()
               .Include(a => a.Directories)
               .Include(a => a.AdderUser)
               .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryPath == ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName) && a.UserId == authenticate.UserId);
                }


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
                directory.UserId = authenticate.UserId;
                directory.Shares = new List<FileDriveShare>();
                var share = await _shareRepository.Where(a => !a.IsDeleted && a.DirectoryId == parentDirectory.DirectoryId).ToListAsync();
                foreach (var item in share)
                    directory.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, EntityType = item.EntityType, UserId = item.UserId, IsDeleted = item.IsDeleted });

                await _directoryRepository.AddAsync(directory);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = (authenticate.language == "en") ? directory.CreatedDate.Value.ToString("yyyy/MM/dd") : directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = (authenticate.language == "en") ? directory.UpdateDate.Value.ToString("yyyy/MM/dd") : directory.UpdateDate.ToPersianDateString(),
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
                _fileHelper.RemoveDirectory(ServiceSetting.PrivateFileDriveRootPath(authenticate.ContractCode, authenticate.UserName), model.Name);
                return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<FileDriverDirectoryListDto>(null, exception);
            }
        }


        public async Task<ServiceResult<bool>> UpdateDirectoryPrivatly(AuthenticateDto authenticate, Guid directoryId, FileDriveDirectoryRenameDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (await dbQuery.AnyAsync(a => a.ParentDirectory.Directories.Any(b =>!b.IsDeleted&& b.DirectoryName == model.Name)))
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

        public async Task<ServiceResult<bool>> DeleteDirectoryPrivatly(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Include(a => a.Directories)
                .Include(a => a.ParentDirectory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var directory = await dbQuery.FirstOrDefaultAsync();

                await DeleteAllSubFileAndDirectoryRecord(directory.DirectoryId);
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

        public async Task<ServiceResult<List<ExpandoObject>>> GetDirectoryTreeAsyncPrivatly(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<ExpandoObject>>(null, MessageId.AccessDenied);

                var dbQuery = _directoryRepository
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId).OrderBy(a => a.CreatedDate);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError<List<ExpandoObject>>(null, MessageId.EntityDoesNotExist);
                var directories = await dbQuery.Select(a => new ObjectTreeDto
                {
                    Value = a.DirectoryId,
                    ParentId = a.ParentId.Value,
                    Label = a.DirectoryName
                }).ToListAsync();

                directories.First(a => a.ParentId == Guid.Empty).Label = "Root";
                var treeView = CreateTreeModelOfDirectories(directories, Guid.Empty).ToList();
                var stringtreeView = JsonConvert.SerializeObject(treeView);
                var result = JsonConvert.DeserializeObject<List<ExpandoObject>>(stringtreeView, new ExpandoObjectConverter());
                return ServiceResultFactory.CreateSuccess<List<ExpandoObject>>(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<ExpandoObject>>(null, exception);
            }
        }
        public async Task<ServiceResult<List<AdvanceSearchDto>>> GetAdvanceSearchDataAsyncPrivatly(AuthenticateDto authenticate)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<List<AdvanceSearchDto>>(null, MessageId.AccessDenied);

                var dbDirectoryQuery = _directoryRepository
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId).OrderBy(a => a.CreatedDate);

                var dbFileQuery = _fileRepository.Include(a => a.Directory)
                .Where(a => !a.IsDeleted && !a.Directory.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.UserId == authenticate.UserId).OrderBy(a => a.CreatedDate);


                var directories = await dbDirectoryQuery.Select(a => new AdvanceSearchDto
                {
                    Id = a.DirectoryId,
                    Name = a.DirectoryName,
                    IsFile = false
                }).ToListAsync();

                var files = await dbFileQuery.Select(a => new AdvanceSearchDto
                {
                    Id = a.FileId,
                    Name = a.FileName,
                    IsFile = true,
                }).ToListAsync();

                var result = new List<AdvanceSearchDto>();
                result.AddRange(directories);
                result.AddRange(files);
                return ServiceResultFactory.CreateSuccess<List<AdvanceSearchDto>>(result);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<List<AdvanceSearchDto>>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> MoveDirectoryAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                if (directoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                if (dbDistinationQuery.Directories.Any(a => !a.IsDeleted && a.DirectoryName == dbQuery.DirectoryName))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectoryInDestination);
                var oldPath = dbQuery.DirectoryPath;

                var newPath = _fileHelper.MoveDirectory(dbQuery.DirectoryPath, dbDistinationQuery.DirectoryPath, dbQuery.DirectoryName);
                int index = dbQuery.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1;
                await ChangeAllSubDirectoryPathRecordForMoveDirectory(dbQuery.DirectoryId, newPath, index,dbDistinationQuery.Shares.Where(a=>!a.IsDeleted&&a.Status==ShareEntityStatus.Active).ToList());
                dbQuery.ParentId = dbDistinationQuery.DirectoryId;
                dbQuery.UserId = authenticate.UserId;

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                _fileHelper.MoveDirectory(oldPath, newPath);
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> CopyDirectoryAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                if (directoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbQuery = await _directoryRepository.Include(a => a.Directories)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                if (dbDistinationQuery.Directories.Any(a => !a.IsDeleted && a.DirectoryName == dbQuery.DirectoryName))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateDirectoryInDestination);
                var oldPath = dbQuery.DirectoryPath;

                var newPath = _fileHelper.CopyDirectory(dbQuery.DirectoryPath, dbDistinationQuery.DirectoryPath, dbQuery.DirectoryName);
                int index = dbQuery.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length - 1;
                var directories = await _directoryRepository.Include(a => a.Files).Where(a => !a.IsDeleted && !a.PermanentDelete && a.UserId == authenticate.UserId).ToListAsync();
                List<FileDriveShare> shareDirectories = new List<FileDriveShare>();
                List<FileDriveShare> shareFiles = new List<FileDriveShare>();
                foreach(var item in dbDistinationQuery.Shares.Where(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active))
                {
                    shareDirectories.Add(new FileDriveShare { EntityType=EntityType.Directory,UserId=item.UserId,Accessablity=item.Accessablity });
                    shareFiles.Add(new FileDriveShare { EntityType = EntityType.File, UserId = item.UserId, Accessablity = item.Accessablity });
                }
                var result = CopyAllSubDirectoryPathRecordForMoveDirectory(directories, dbQuery.DirectoryId, dbQuery.ParentId, newPath, index, 0, shareDirectories,shareFiles).ToList();
                result[0].ParentId = destinationId;
                result[0].UserId = authenticate.UserId;
                await _directoryRepository.AddAsync(result[0]);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    return ServiceResultFactory.CreateSuccess(true);
                }

                _fileHelper.MoveDirectory(oldPath, newPath);
                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<DownloadFileDto> DownloadFolderAsyncPrivatly(AuthenticateDto authenticate, Guid directoryId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = await _directoryRepository.Where(a => !a.IsDeleted && a.DirectoryId == directoryId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;

                return await _fileHelper.DownloadFileDriveFolder(dbQuery.DirectoryPath);

            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public async Task<ServiceResult<FileDriverDirectoryListDto>> FileDriveUploadFolderPrivatly(AuthenticateDto authenticate, Guid directoryId, IFormFileCollection files)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriverDirectoryListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null && a.UserId == authenticate.UserId).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId && a.UserId == authenticate.UserId).FirstOrDefaultAsync();

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

                var share = await _shareRepository.Where(a => !a.IsDeleted && a.DirectoryId == dbQuery.DirectoryId).ToListAsync();
                var directory = PrivateDirectoryUpload(_hostingEnvironmentRoot.ContentRootPath + dbQuery.DirectoryPath + parrentDirectory + "/", authenticate.ContractCode, authenticate.UserId, share);
                directory.ParentId = dbQuery.DirectoryId;
                directory.UserId = authenticate.UserId;



                await _directoryRepository.AddAsync(directory);
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new FileDriverDirectoryListDto
                    {
                        CreateDate = (authenticate.language == "en") ? directory.CreatedDate.Value.ToString("yyyy/MM/dd") : directory.CreatedDate.ToPersianDateString(),
                        ModifiedDate = (authenticate.language == "en") ? directory.UpdateDate.Value.ToString("yyyy/MM/dd") : directory.UpdateDate.ToPersianDateString(),
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

        public async Task<ServiceResult<bool>> AddSharePrivateAsync(AuthenticateDto authenticate, FileDriveShareCreateDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);


                List<FileDriveShare> result = new List<FileDriveShare>();

                if ((model.EntityType == EntityType.Directory && await _shareRepository.AnyAsync(a => a.DirectoryId == model.EntityId)) || (model.EntityType == EntityType.File && await _shareRepository.AnyAsync(a => a.FileId == model.EntityId)))
                {
                    if (model.EntityType == EntityType.Directory && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.DirectoryId == model.EntityId))
                        return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                    if (model.EntityType == EntityType.File && !await _shareRepository.AnyAsync(a => !a.IsDeleted && a.UserId == authenticate.UserId && a.Accessablity == Accessablity.Editor && a.FileId == model.EntityId))
                        return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);
                    var share = (model.EntityType == EntityType.Directory) ? await _shareRepository.Where(a => a.DirectoryId == model.EntityId).ToListAsync() : await _shareRepository.Where(a => a.FileId == model.EntityId).ToListAsync();
                    List<FileDriveShare> extraShare = new List<FileDriveShare>();

                    foreach (var item in model.Owners)
                    {
                        model.Users.Add(new FileDriveShareUserAccessablityDto { UserId = item.UserId, Access = new Access { Value = item.Accessablity, Label = "" } });
                    }

                    List<FileDriveShareUserAccessablityDto> deletedItems = new List<FileDriveShareUserAccessablityDto>();
                    foreach (var item in share)
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
                        var index = share.FindIndex(a => a.UserId == item.UserId);
                        if (index >= 0)
                        {
                            share[index].IsDeleted = false;
                            share[index].Accessablity = item.Access.Value;
                        }
                        else
                        {
                            if (model.EntityType == EntityType.Directory)
                            {

                                await AddSharingRecordForAllSubDirectory(model.EntityId, new List<FileDriveShareUserAccessablityDto> { item });
                            }

                            else
                                extraShare.Add(new FileDriveShare { Accessablity = item.Access.Value, FileId = model.EntityId, EntityType = model.EntityType, UserId = item.UserId, IsDeleted = false });
                        }
                    }
                    if (deletedItems.Any())
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
                    if (directory.AdderUserId != authenticate.UserId && directory.UserId != authenticate.UserId)
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




                    await AddSharingRecordForAllSubDirectory(model.EntityId, model.Users);




                }
                else if (model.EntityType == EntityType.File)
                {
                    var dbQuery = _fileRepository.Where(a => !a.IsDeleted && a.FileId == model.EntityId && a.Directory.ContractCode == authenticate.ContractCode);

                    var file = await dbQuery.FirstOrDefaultAsync();
                    if (file == null)
                        return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                    if (file.AdderUserId != authenticate.UserId && file.UserId != authenticate.UserId)
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

        #endregion
        private async Task AddSharingRecordForAllSubDirectory(Guid directoryId, List<FileDriveShareUserAccessablityDto> users)
        {

            try
            {

                var directory = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Include(a => a.Shares).FirstAsync(a => a.DirectoryId == directoryId);
                foreach (var user in users)
                {
                    if (directory.Shares != null)
                        directory.Shares.Add(new FileDriveShare { Accessablity = user.Access.Value, EntityType = EntityType.Directory, UserId = user.UserId });
                    else
                        directory.Shares = new List<FileDriveShare> { new FileDriveShare { Accessablity = user.Access.Value, EntityType = EntityType.Directory, UserId = user.UserId } };
                    foreach (var file in directory.Files)
                    {
                        var files = await _fileRepository.Include(a => a.Shares).FirstAsync(a => a.FileId == file.FileId);
                        if (files.Shares != null)
                            files.Shares.Add(new FileDriveShare { Accessablity = user.Access.Value, EntityType = EntityType.File, UserId = user.UserId });
                        else
                            files.Shares = new List<FileDriveShare> { new FileDriveShare { Accessablity = user.Access.Value, EntityType = EntityType.File, UserId = user.UserId } };
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
                    foreach (var item in directory.Shares.Where(a => a.UserId == user.UserId))
                        item.IsDeleted = true;

                    foreach (var file in directory.Files)
                    {
                        var files = await _fileRepository.Include(a => a.Shares).FirstAsync(a => a.FileId == file.FileId);
                        foreach (var item in files.Shares.Where(a => a.UserId == user.UserId))
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
        private async Task<List<FileDriveBreadcrumbDto>> CreateBreadcrumb(string directoryPath)
        {
            var result = new List<FileDriveBreadcrumbDto>();
            var directories = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (directories.Length > 4)
            {
                var fixedDir = "/" + directories[0] + "/" + directories[1] + "/" + directories[2] + "/" + directories[3] + "/";
                for (int i = 4; i < directories.Length; i++)
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

        private async Task<List<FileDriveBreadcrumbDto>> CreateBreadcrumbPrivate(string directoryPath)
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
        private async Task DeleteAllSubFileAndDirectoryRecord(Guid directoryId)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Include(a => a.Shares).FirstAsync(a => a.DirectoryId == directoryId);
                directory.IsDeleted = true;
                foreach (var item in directory.Shares)
                    item.Status = ShareEntityStatus.IsOwnerTrash;
                foreach (var file in directory.Files)
                {
                    var shares = await _shareRepository.Where(a => a.FileId == file.FileId).ToListAsync();
                    foreach (var item in shares)
                        item.Status = ShareEntityStatus.IsOwnerTrash;
                    file.IsDeleted = true;
                }

                foreach (var dir in directory.Directories)
                    await DeleteAllSubFileAndDirectoryRecord(dir.DirectoryId);

            }
            catch (Exception ex)
            {

            }
        }
        private async Task DeletePermanentAllSubFileAndDirectoryRecord(Guid directoryId)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares).Include(a => a.Files).FirstAsync(a => a.DirectoryId == directoryId);
                directory.PermanentDelete = true;
                directory.IsDeleted = true;
                foreach (var item in directory.Shares)
                {
                    item.IsDeleted = true;
                    item.Status = ShareEntityStatus.IsOwnerTrash;
                }
                   
                foreach (var file in directory.Files)
                {
                    var share = await _shareRepository.Where(a => a.FileId == file.FileId).ToListAsync();
                    foreach(var item in file.Shares)
                    {
                        item.IsDeleted = true;
                        item.Status = ShareEntityStatus.IsOwnerTrash;
                    }
                    file.IsDeleted = true;
                    file.PermanentDelete = true;
                }

                foreach (var dir in directory.Directories)
                    await DeletePermanentAllSubFileAndDirectoryRecord(dir.DirectoryId);

            }
            catch (Exception ex)
            {

            }
        }
        private async Task RestoreAllSubFileAndDirectoryRecord(Guid directoryId)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares).Include(a => a.Files).FirstAsync(a => a.DirectoryId == directoryId);
                directory.IsDeleted = false;
                foreach (var item in directory.Shares)
                    item.Status = ShareEntityStatus.Active;
                foreach (var file in directory.Files)
                {
                    var shares = await _shareRepository.Where(a => a.FileId == file.FileId).ToListAsync();
                    foreach (var item in shares)
                        item.Status = ShareEntityStatus.Active;
                    file.IsDeleted = false;
                }
                foreach (var dir in directory.Directories)
                    await RestoreAllSubFileAndDirectoryRecord(dir.DirectoryId);

            }
            catch (Exception ex)
            {

            }
        }
        private async Task ChangeAllSubDirectoryPathRecordForMoveDirectory(Guid directoryId, string newName, int index,List<FileDriveShare> shares)
        {

            try
            {
                var directory = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Files).FirstAsync(a => a.DirectoryId == directoryId);
                var paths = directory.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                directory.DirectoryPath = (index < paths.Length - 1) ? newName + String.Join('/', paths, index + 1, (paths.Length - 1) - index) + "/" : newName;
                directory.Shares = new List<FileDriveShare>();
                foreach (var item in shares)
                    directory.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, UserId = item.UserId, EntityType = EntityType.Directory });
                foreach (var file in directory.Files.Where(a => !a.PermanentDelete))
                {
                    file.Shares = new List<FileDriveShare>();
                    foreach (var item in shares)
                        file.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, UserId = item.UserId, EntityType = EntityType.File });
                }
                foreach (var dir in directory.Directories)
                    await ChangeAllSubDirectoryPathRecordForMoveDirectory(dir.DirectoryId, newName, index,shares);

            }
            catch (Exception ex)
            {

            }
        }


        private IEnumerable<FileDriveDirectory> CopyAllSubDirectoryPathRecordForMoveDirectory(List<FileDriveDirectory> directoryModel, Guid directoryId, Guid? parentId, string newName, int index, int count,List<FileDriveShare> shareDirectories,List<FileDriveShare> shareFiles)
        {



            var directory = directoryModel.First(a => a.DirectoryId == directoryId);

            if (count == 0)
            {
                foreach (var dir in directoryModel.Where(a => a.ParentId == parentId && a.DirectoryId == directoryId))
                {
                    var paths = dir.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    
                    yield return new FileDriveDirectory
                    {
                        ContractCode = dir.ContractCode,
                        DirectoryName = dir.DirectoryName,
                        UserId = dir.UserId,
                        Shares=shareDirectories,
                        DirectoryPath = (index < paths.Length - 1) ? newName + String.Join('/', paths, index + 1, (paths.Length - 1) - index) + "/" : newName,
                        Files = dir.Files.Select(b => new FileDriveFile
                        {
                            FileName = b.FileName,
                            FileSize = b.FileSize,
                            IsDeleted = b.IsDeleted,
                            UserId = b.UserId,
                            Shares=shareFiles
                        }).ToList(),
                        Directories = CopyAllSubDirectoryPathRecordForMoveDirectory(directoryModel, dir.DirectoryId, Guid.Empty, newName, index, 1, shareDirectories, shareFiles).ToList()
                    };
                }
            }
            else if (directory.Directories != null && directory.Directories.Any())
            {
                foreach (var dir in directoryModel.Where(a => a.ParentId == directoryId))
                {
                    var paths = dir.DirectoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    yield return new FileDriveDirectory
                    {
                        ContractCode = dir.ContractCode,
                        DirectoryName = dir.DirectoryName,
                        UserId = dir.UserId,
                        Shares = shareDirectories,
                        DirectoryPath = (index < paths.Length - 1) ? newName + String.Join('/', paths, index + 1, (paths.Length - 1) - index) + "/" : newName,
                        Files = dir.Files.Select(b => new FileDriveFile
                        {
                            FileName = b.FileName,
                            FileSize = b.FileSize,
                            IsDeleted = b.IsDeleted,
                            UserId = b.UserId,
                            Shares = shareFiles
                        }).ToList(),
                        Directories = CopyAllSubDirectoryPathRecordForMoveDirectory(directoryModel, dir.DirectoryId, Guid.Empty, newName, index, 1,shareDirectories,shareFiles).ToList()
                    };
                }
            }

        }



        private FileDriveDirectory DirectoryUpload(string sourceDirName, string contractCode)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            FileDriveDirectory directory = new FileDriveDirectory();
            var finalPath = (dir.FullName.Replace('\\', '/').LastIndexOf('/') == dir.FullName.Length - 1) ? "" : "/";
            directory.ContractCode = contractCode;
            directory.DirectoryName = dir.Name;
            directory.DirectoryPath = dir.FullName.Substring(dir.FullName.IndexOf("\\Files\\")) + finalPath;
            directory.DirectoryPath = directory.DirectoryPath.Replace("\\", "/");
            FileInfo[] files = dir.GetFiles();
            directory.Directories = new List<FileDriveDirectory>();
            directory.Files = new List<FileDriveFile>();
            foreach (FileInfo file in files)
            {

                directory.Files.Add(new FileDriveFile { FileName = file.Name, FileSize = file.Length });
            }
            DirectoryInfo[] dirs = dir.GetDirectories();


            foreach (DirectoryInfo subdir in dirs)
            {

                directory.Directories.Add(DirectoryUpload(subdir.FullName, contractCode));
            }
            return directory;
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
                            file.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, EntityType = EntityType.File, UserId = item.UserId, IsDeleted = item.IsDeleted });

                    }
            }
            DirectoryInfo[] dirs = dir.GetDirectories();


            foreach (DirectoryInfo subdir in dirs)
            {

                directory.Directories.Add(PrivateDirectoryUpload(subdir.FullName, contractCode, userId, share));
            }
            return directory;
        }

        private IEnumerable<FinalObjectTreeModelDto> CreateTreeModelOfDirectories(List<ObjectTreeDto> treeModel, Guid root)
        {






            foreach (var dir in treeModel.Where(a => a.ParentId == root))
            {

                yield return new FinalObjectTreeModelDto
                {
                    Label = dir.Label,
                    Value = dir.Value,
                    ShowCheckbox = false,
                    //Icon = "<i className='fa fa-folder fa-lg text-warning'></i>",
                    //ClassName = "TreeClass",
                    Disabled = false,
                    Children = CreateTreeModelOfDirectories(treeModel, dir.Value).ToList()

                };
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

    }
}
