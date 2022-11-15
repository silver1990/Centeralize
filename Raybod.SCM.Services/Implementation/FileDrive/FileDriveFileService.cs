using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.FileDriveDirectory;
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
    public class FileDriveFileService : IFileDriveFileService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<FileDriveDirectory> _directoryRepository;
        private readonly DbSet<FileDriveFile> _fileRepository;
        private readonly DbSet<FileDriveShare> _shareRepository;
        private readonly DbSet<RevisionAttachment> _revisionAttachmentRepository;
        private readonly DbSet<User> _userRepository;
        private readonly ITeamWorkAuthenticationService _authenticationService;
        private readonly CompanyConfig _appSettings;
        private readonly Utilitys.FileHelper _fileHelper;
        private readonly IWebHostEnvironment _hostingEnvironmentRoot;
        public FileDriveFileService(IUnitOfWork unitOfWork,
            ITeamWorkAuthenticationService authenticationService,
            IOptions<CompanyAppSettingsDto> appSettings,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironmentRoot)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
            _directoryRepository = _unitOfWork.Set<FileDriveDirectory>();
            _fileRepository = _unitOfWork.Set<FileDriveFile>();
            _shareRepository = _unitOfWork.Set<FileDriveShare>();
            _revisionAttachmentRepository = _unitOfWork.Set<RevisionAttachment>();
            _userRepository = _unitOfWork.Set<User>();
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _hostingEnvironmentRoot = hostingEnvironmentRoot;
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("companyCode", out var CompanyCode);
            _appSettings = appSettings.Value.CompanyConfig.First(a => a.CompanyCode == CompanyCode);
        }

        #region Public
        public async Task<DownloadFileDto> FileDriveDownloadFile(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
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
        public async Task<DownloadFileDto> FileDrivePreviewFile(Guid fileId)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

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
        public async Task<DownloadFileDto> GetPreviewFile(string fileSrc)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

                var dbQuery = await _revisionAttachmentRepository.Include(a=>a.DocumentRevision).ThenInclude(a=>a.Document).Include(a=>a.ConfirmationWorkFlow).ThenInclude(a => a.DocumentRevision).ThenInclude(a => a.Document).Where(a => !a.IsDeleted && a.FileSrc == fileSrc).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;

                var contractCode = (dbQuery.DocumentRevision != null) ? dbQuery.DocumentRevision.Document.ContractCode : dbQuery.ConfirmationWorkFlow.DocumentRevision.Document.ContractCode;
                var revisionId = (dbQuery.DocumentRevision != null) ? dbQuery.DocumentRevisionId : dbQuery.ConfirmationWorkFlow.DocumentRevisionId;
                var documentId= (dbQuery.DocumentRevision != null) ? dbQuery.DocumentRevision.DocumentId : dbQuery.ConfirmationWorkFlow.DocumentRevision.DocumentId;

                return await _fileHelper.DownloadDocumentForPreview(fileSrc, ServiceSetting.UploadFilePathDocument(contractCode, documentId, revisionId.Value));

            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadFile(AuthenticateDto authenticate, Guid directoryId, IFormFile file)
        {


            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null && a.UserId == null).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Files).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId).FirstOrDefaultAsync();

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




                var saveResult = await _fileHelper.FileDriveSaveDocument(file, dbQuery.DirectoryPath, fileName);

                if (dbQuery.Files != null && dbQuery.Files.Any())
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length });
                else
                {
                    dbQuery.Files = new List<FileDriveFile>();
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length });
                }
                if (saveResult)
                {
                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {
                        var result = dbQuery.Files.OrderByDescending(a => a.CreatedDate).Select(a => new FileDriveFilesListDto
                        {
                            CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                            Extension = Path.GetExtension(a.FileName).Substring(1),
                            Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                            Id = a.FileId,
                            ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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

        public async Task<ServiceResult<bool>> DeleteFile(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var file = await dbQuery.FirstOrDefaultAsync();

                file.IsDeleted = true;

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
        public async Task<ServiceResult<bool>> DeleteFilePermanently(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                IQueryable<FileDriveFile> dbQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPrivatePermission&&!permission.HasPublicPermission)
                {
                     dbQuery = _fileRepository
               .Include(a => a.Directory)
               .Include(a => a.AdderUser)
               .Include(a=>a.Shares)
               .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (a.UserId == authenticate.UserId|| _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (!permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                     dbQuery = _fileRepository
               .Include(a => a.Directory)
               .Include(a => a.AdderUser)
               .Include(a => a.Shares)
               .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (a.UserId == null || _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _fileRepository
              .Include(a => a.Directory)
              .Include(a => a.AdderUser)
              .Include(a => a.Shares)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (a.UserId == null || a.UserId == authenticate.UserId || _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else 
                {
                    dbQuery = _fileRepository
              .Include(a => a.Directory)
              .Include(a => a.AdderUser)
              .Where(a => a.IsDeleted && !a.PermanentDelete && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && ( _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }




                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var file = await dbQuery.FirstOrDefaultAsync();

                file.PermanentDelete = true;
                file.IsDeleted = true;
                foreach(var item in file.Shares)
                {
                    item.IsDeleted = true;
                    item.Status = ShareEntityStatus.IsOwnerTrash;
                }
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.DeleteDocumentFromPath(ServiceSetting.FileDriveTrashPath(authenticate.ContractCode) + (file.FileId+Path.GetExtension(file.FileName)));
                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }


        public async Task<ServiceResult<bool>> RestoreFile(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                IQueryable<FileDriveFile> dbQuery;
                var permission = await _authenticationService.HasUserPermissionForFileDriveTrash(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (permission.HasPrivatePermission&& !permission.HasPublicPermission)
                {
                     dbQuery = _fileRepository
                    .Include(a => a.Directory)
                    .Include(a => a.AdderUser)
                    .Include(a=>a.Shares)
                    .Where(a => !a.PermanentDelete && a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId &&  (a.UserId == authenticate.UserId||_shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if(!permission.HasPrivatePermission&&permission.HasPublicPermission)
                {
                     dbQuery = _fileRepository
                    .Include(a => a.Directory)
                    .Include(a => a.AdderUser)
                    .Include(a => a.Shares)
                    .Where(a => !a.PermanentDelete && a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (a.UserId == null || _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }
                else if (permission.HasPrivatePermission && permission.HasPublicPermission)
                {
                    dbQuery = _fileRepository
                   .Include(a => a.Directory)
                   .Include(a => a.AdderUser)
                   .Include(a => a.Shares)
                   .Where(a => !a.PermanentDelete && a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (a.UserId == null || a.UserId == authenticate.UserId|| _shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }

                else
                {
                    dbQuery = _fileRepository
                   .Include(a => a.Directory)
                   .Include(a => a.AdderUser)
                   .Include(a => a.Shares)
                   .Where(a => !a.PermanentDelete && a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId && (_shareRepository.Any(b => b.FileId == fileId && b.UserId == a.AdderUserId && (b.Status == ShareEntityStatus.IsEditorTrash || b.Status == ShareEntityStatus.IsOwnerTrash))));
                }


                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var file = await dbQuery.FirstOrDefaultAsync();
                if (_fileHelper.IsFileExist(file.Directory.DirectoryPath + file.FileName))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateFile);

                file.IsDeleted = false;
                foreach (var item in file.Shares)
                    item.Status = ShareEntityStatus.Active;
                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    _fileHelper.RestoreFileFromTrash(file.Directory.DirectoryPath + file.FileName, file.FileId.ToString() + Path.GetExtension(file.FileName), authenticate.ContractCode);

                    return ServiceResultFactory.CreateSuccess(true);
                }

                return ServiceResultFactory.CreateError(false, MessageId.SaveFailed);

            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> UpdateFile(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .ThenInclude(a=>a.Files)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var file = await dbQuery.FirstOrDefaultAsync();
                var extension = Path.GetExtension(file.FileName);
                if ( file.Directory.Files.Any(b =>!b.IsDeleted&& b.FileName == model.Name+extension))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateFile);
               

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

        public async Task<ServiceResult<bool>> MoveFileAsync(AuthenticateDto authenticate, Guid fileId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);



                var dbQuery = await _fileRepository.Include(a => a.Directory)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (dbQuery.Directory.DirectoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);



                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                string fileName = dbQuery.FileName;
                int counter = 1;
                while (true)
                {
                    if (dbDistinationQuery.Files.Any(a =>!a.IsDeleted&& a.FileName == fileName))
                    {
                        fileName = dbQuery.FileName.Substring(0, dbQuery.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(dbQuery.FileName);
                        counter++;
                    }
                    else
                        break;
                }


                if (!_fileHelper.MoveFile(dbQuery.Directory.DirectoryPath, dbDistinationQuery.DirectoryPath, fileName,dbQuery.FileName))
                    return ServiceResultFactory.CreateError(false, MessageId.MoveFileFailed);
                dbQuery.FileName = fileName;
                dbQuery.DirectoryId = dbDistinationQuery.DirectoryId;


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

        public async Task<ServiceResult<bool>> CopyFileAsync(AuthenticateDto authenticate, Guid fileId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);




                var dbQuery = await _fileRepository.Include(a => a.Directory)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (dbQuery.Directory.DirectoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

               

                string fileName = dbQuery.FileName;
                int counter = 1;
                while (true)
                {
                    if (dbDistinationQuery.Files.Any(a =>!a.IsDeleted&& a.FileName == fileName))
                    {
                        fileName = dbQuery.FileName.Substring(0, dbQuery.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(dbQuery.FileName);
                        counter++;
                    }
                    else
                        break;
                }


                if (!_fileHelper.CopyFile(dbQuery.Directory.DirectoryPath, dbDistinationQuery.DirectoryPath, fileName,dbQuery.FileName))
                    return ServiceResultFactory.CreateError(false, MessageId.CopyFileFailed);
                if (dbDistinationQuery.Files != null && dbDistinationQuery.Files.Any())
                    dbDistinationQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = dbQuery.FileSize });
                else
                    dbDistinationQuery.Files = new List<FileDriveFile> { new FileDriveFile { FileName = fileName, FileSize = dbQuery.FileSize } };
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

        #endregion

        #region Private

        public async Task<DownloadFileDto> FileDriveDownloadFilePrivate(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return null;

                var dbQuery = await _fileRepository.Include(a => a.Directory).Where(a => !a.IsDeleted && a.FileId == fileId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return null;

                return await _fileHelper.DownloadFileDriveDocument(dbQuery.Directory.DirectoryPath, dbQuery.FileName);

            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public async Task<DownloadFileDto> FileDrivePreviewFilePrivate(Guid fileId)
        {
            try
            {
                //var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                //if (!permission.HasPermission)
                //    return null;

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
        public async Task<ServiceResult<FileDriveFilesListDto>> FileDriveUploadFilePrivate(AuthenticateDto authenticate, Guid directoryId, IFormFile file)
        {


            try
            {

                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<FileDriveFilesListDto>(null, MessageId.AccessDenied);
                FileDriveDirectory dbQuery;
                if (directoryId == Guid.Empty)
                    dbQuery = await _directoryRepository.Include(a => a.Files).Include(a=>a.Shares).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.ParentId == null&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();
                else
                    dbQuery = await _directoryRepository.Include(a => a.Files).Include(a => a.Shares).Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == directoryId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

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
                    if (dbQuery.Files.Any(a =>!a.IsDeleted&& a.FileName == fileName))
                    {
                        fileName = file.FileName.Substring(0, file.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(file.FileName);
                        counter++;
                    }
                    else
                        break;
                }


                List<FileDriveShare> shares = new List<FileDriveShare>();
                if (dbQuery.Shares != null && dbQuery.Shares.Any(a => !a.IsDeleted))
                    foreach (var item in dbQuery.Shares)
                        shares.Add(new FileDriveShare { EntityType = EntityType.File, UserId = item.UserId, Accessablity = item.Accessablity });
                var saveResult = await _fileHelper.FileDriveSaveDocument(file, dbQuery.DirectoryPath, fileName);

                if (dbQuery.Files != null && dbQuery.Files.Any())
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length,UserId=authenticate.UserId,Shares= shares });
                else
                {
                    dbQuery.Files = new List<FileDriveFile>();
                    dbQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = file.Length, UserId = authenticate.UserId,Shares=shares });
                }
                if (saveResult)
                {
                    if (await _unitOfWork.SaveChangesAsync() > 0)
                    {
                        var result = dbQuery.Files.OrderByDescending(a => a.CreatedDate).Select(a => new FileDriveFilesListDto
                        {
                            CreateDate = (authenticate.language == "en") ? a.CreatedDate.Value.ToString("yyyy/MM/dd") : a.CreatedDate.ToPersianDateString(),
                            Extension = Path.GetExtension(a.FileName).Substring(1),
                            Name = a.FileName.Substring(0, a.FileName.IndexOf('.')),
                            Id = a.FileId,
                            ModifiedDate = (authenticate.language == "en") ? a.UpdateDate.Value.ToString("yyyy/MM/dd") : a.UpdateDate.ToPersianDateString(),
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

        public async Task<ServiceResult<bool>> DeleteFilePrivate(AuthenticateDto authenticate, Guid fileId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .Include(a => a.AdderUser)
                .Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId&&a.UserId==authenticate.UserId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                var file = await dbQuery.FirstOrDefaultAsync();
                foreach (var item in file.Shares)
                    item.Status = ShareEntityStatus.IsOwnerTrash;
                file.IsDeleted = true;

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

        public async Task<ServiceResult<bool>> UpdateFilePrivate(AuthenticateDto authenticate, Guid fileId, FileDriveFileRenameDto model)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = _fileRepository
                .Include(a => a.Directory)
                .ThenInclude(a=>a.Files)
                .Include(a => a.AdderUser)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId&&a.UserId==authenticate.UserId);



                if (await dbQuery.CountAsync() == 0)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                var file = await dbQuery.FirstOrDefaultAsync();
                var extension = Path.GetExtension(file.FileName);
                if (file.Directory.Files.Any(b => !b.IsDeleted && b.FileName == model.Name + extension))
                    return ServiceResultFactory.CreateError(false, MessageId.DuplicateFile);
               

                if (!_fileHelper.RenameFile(file.Directory.DirectoryPath, file.FileName, model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.EditEntityFailed);
                if(!_fileHelper.ValidateTitle(model.Name))
                    return ServiceResultFactory.CreateError(false, MessageId.InvalidCharacter);
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

        public async Task<ServiceResult<bool>> MoveFileAsyncPrivate(AuthenticateDto authenticate, Guid fileId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);



                var dbQuery = await _fileRepository.Include(a => a.Directory).Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (dbQuery.Directory.DirectoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);



                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a=>a.Shares).Include(a => a.Files)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

                string fileName = dbQuery.FileName;
                int counter = 1;
                while (true)
                {
                    if (dbDistinationQuery.Files.Any(a =>!a.IsDeleted&& a.FileName == fileName))
                    {
                        fileName = dbQuery.FileName.Substring(0, dbQuery.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(dbQuery.FileName);
                        counter++;
                    }
                    else
                        break;
                }


                if (!_fileHelper.MoveFile(dbQuery.Directory.DirectoryPath, dbDistinationQuery.DirectoryPath, fileName,dbQuery.FileName))
                    return ServiceResultFactory.CreateError(false, MessageId.MoveFileFailed);
                dbQuery.FileName = fileName;
                dbQuery.DirectoryId = dbDistinationQuery.DirectoryId;
                dbQuery.Shares = new List<FileDriveShare>();
                foreach (var item in dbDistinationQuery.Shares.Where(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active))
                    dbQuery.Shares.Add(new FileDriveShare { Accessablity = item.Accessablity, EntityType = EntityType.File, Status = item.Status,UserId=item.UserId });

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

        public async Task<ServiceResult<bool>> CopyFileAsyncPrivate(AuthenticateDto authenticate, Guid fileId, Guid destinationId)
        {
            try
            {
                var permission = await _authenticationService.HasUserPermissionWithProductGroup(authenticate.UserId, authenticate.ContractCode, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var dbQuery = await _fileRepository.Include(a => a.Directory)
                .Where(a => !a.IsDeleted && a.Directory.ContractCode == authenticate.ContractCode && a.FileId == fileId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

                if (dbQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                if (dbQuery.Directory.DirectoryId == destinationId)
                    return ServiceResultFactory.CreateError(false, MessageId.SourceAndDestinationIsSame);

                var dbDistinationQuery = await _directoryRepository.Include(a => a.Directories).Include(a => a.Files).Include(a=>a.Shares)
                .Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode && a.DirectoryId == destinationId&&a.UserId==authenticate.UserId).FirstOrDefaultAsync();

                if (dbDistinationQuery == null)
                    return ServiceResultFactory.CreateError(false, MessageId.DestinationDirectoryNotExist);

              

                string fileName = dbQuery.FileName;
                int counter = 1;
                while (true)
                {
                    if (dbDistinationQuery.Files.Any(a =>!a.IsDeleted&& a.FileName == fileName))
                    {
                        fileName = dbQuery.FileName.Substring(0, dbQuery.FileName.IndexOf(".")) + $"({counter.ToString()})" + Path.GetExtension(dbQuery.FileName);
                        counter++;
                    }
                    else
                        break;
                }

                List<FileDriveShare> shares = new List<FileDriveShare>();
                foreach (var item in dbDistinationQuery.Shares.Where(a => !a.IsDeleted && a.Status == ShareEntityStatus.Active))
                    shares.Add(new FileDriveShare { Accessablity=item.Accessablity,Status = item.Status, EntityType = EntityType.File, UserId = item.UserId });
                if (!_fileHelper.CopyFile(dbQuery.Directory.DirectoryPath, dbDistinationQuery.DirectoryPath, fileName, dbQuery.FileName))
                    return ServiceResultFactory.CreateError(false, MessageId.CopyFileFailed);
                if (dbDistinationQuery.Files != null && dbDistinationQuery.Files.Any())
                    dbDistinationQuery.Files.Add(new FileDriveFile { FileName = fileName, FileSize = dbQuery.FileSize,UserId=authenticate.UserId,Shares= shares });
                else
                    dbDistinationQuery.Files = new List<FileDriveFile> { new FileDriveFile { FileName = fileName, FileSize = dbQuery.FileSize,UserId=authenticate.UserId, Shares = shares } };
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
        #endregion

    }
}
