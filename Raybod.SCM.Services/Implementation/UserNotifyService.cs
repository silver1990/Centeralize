using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Notification;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.Struct;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class UserNotifyService : IUserNotifyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamWorkAuthenticationService _authenticationServices;
        private readonly DbSet<UserNotify> _userNotifyRepository;
        private readonly DbSet<Role> _roleRepository;

        public UserNotifyService(IUnitOfWork unitOfWork, ITeamWorkAuthenticationService authenticationServices)
        {
            _unitOfWork = unitOfWork;
            _userNotifyRepository = _unitOfWork.Set<UserNotify>();
            _roleRepository = _unitOfWork.Set<Role>();
            _authenticationServices = authenticationServices;
        }


        public async Task<ServiceResult<UserNotifyListDto>> GetUserNotifies(AuthenticateDto authenticate)

        {
            try
            {

                var emailNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.NotifyType == NotifyManagementType.Email && a.TeamWork.ContractCode == authenticate.ContractCode).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    SubModuleName = a.SubModuleName,
                    NotifyType = a.NotifyType
                }).ToListAsync();
                var reminderNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.NotifyType == NotifyManagementType.Task && a.TeamWork.ContractCode == authenticate.ContractCode).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    SubModuleName = a.SubModuleName,
                    NotifyType = a.NotifyType
                }).ToListAsync();
                var eventNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.NotifyType == NotifyManagementType.Event && a.TeamWork.ContractCode == authenticate.ContractCode).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    SubModuleName = a.SubModuleName,
                    NotifyType = a.NotifyType
                }).ToListAsync();

                UserNotifyListDto result = new UserNotifyListDto();
                result.Emails = PrepareUserNotify(emailNotifies, authenticate.language,false);
                result.Events = PrepareUserNotify(eventNotifies, authenticate.language,true);
                result.Reminders = PrepareUserNotify(reminderNotifies, authenticate.language,false);






                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<UserNotifyListDto>(null, ex);
            }
        }
        public async Task<ServiceResult<UserNotifyListDto>> GetUserNotifies(AuthenticateDto authenticate, int teamworkId, int userId)

        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError<UserNotifyListDto>(null, MessageId.AccessDenied);



                var emailNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.NotifyType == NotifyManagementType.Email && a.TeamWorkId == teamworkId).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    NotifyType = a.NotifyType,
                    SubModuleName = a.SubModuleName,
                }).ToListAsync();

                var eventNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.NotifyType == NotifyManagementType.Event && a.TeamWorkId == teamworkId).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    NotifyType = a.NotifyType,
                    SubModuleName = a.SubModuleName,
                }).ToListAsync();
                var reminderNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.NotifyType == NotifyManagementType.Task && a.TeamWorkId == teamworkId).Select(a => new UserNotifyWithSubDto
                {
                    Id = a.Id,
                    NotifyNumber = a.NotifyNumber,
                    IsActive = a.IsActive,
                    NotifyType = a.NotifyType,
                    SubModuleName = a.SubModuleName,
                }).ToListAsync();
                UserNotifyListDto result = new UserNotifyListDto();
                result.Emails = PrepareUserNotify(emailNotifies, authenticate.language,false);
                result.Events = PrepareUserNotify(eventNotifies, authenticate.language,true);
                result.Reminders = PrepareUserNotify(reminderNotifies, authenticate.language,false);
                return ServiceResultFactory.CreateSuccess(result);
            }
            catch (Exception ex)
            {
                return ServiceResultFactory.CreateException<UserNotifyListDto>(null, ex);
            }
        }
        public async Task<ServiceResult<bool>> UpdateUserNotifies(AuthenticateDto authenticate, UserNotifyListDto model, int teamworkId, int userId)
        {
            try
            {
                var permission = await _authenticationServices.HasUserPermissionWithProductGroup(authenticate.UserId, null, authenticate.Roles);
                if (!permission.HasPermission)
                    return ServiceResultFactory.CreateError(false, MessageId.AccessDenied);

                var emails = (model.Emails != null) ? model.Emails.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                var events = (model.Events != null) ? model.Events.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                if (events.Any(a => RelatedEvents.documentExeptions.Contains(a.NotifyNumber) || RelatedEvents.constructionExeptions.Contains(a.NotifyNumber)))
                {
                    var related = events.Where(a => RelatedEvents.documentExeptions.Contains(a.NotifyNumber) || RelatedEvents.constructionExeptions.Contains(a.NotifyNumber)).ToList();
                    events.AddRange(HandleRelatedEvents(related));
                }
                var reminders = (model.Reminders != null) ? model.Reminders.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                
                var emailNotifyNumbers = emails.Select(a => a.NotifyNumber).ToList();
                var eventNotifyNumbers = events.Select(a => a.NotifyNumber).ToList();
                var reminderNotifyNumbers = reminders.Select(a => a.NotifyNumber).ToList();

                var emailNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.TeamWorkId == teamworkId && a.NotifyType == NotifyManagementType.Email && emailNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                var eventNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.TeamWorkId == teamworkId && a.NotifyType == NotifyManagementType.Event && eventNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                var reminderNotifies = await _userNotifyRepository.Where(a => a.UserId == userId && a.TeamWorkId == teamworkId && a.NotifyType == NotifyManagementType.Task && reminderNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();


                foreach (var item in emailNotifies)
                {

                    item.IsActive = emails.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                }
                foreach (var item in eventNotifies)
                {
                    item.IsActive = events.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                }
                foreach (var item in reminderNotifies)
                {
                    item.IsActive = reminders.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                }
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
        public async Task<ServiceResult<bool>> UpdateUserNotifies(AuthenticateDto authenticate, UserNotifyListDto model)
        {
            try
            {
                var emails = (model.Emails != null) ? model.Emails.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                var events = (model.Events != null) ? model.Events.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                if (events.Any(a => RelatedEvents.documentExeptions.Contains(a.NotifyNumber) || RelatedEvents.constructionExeptions.Contains(a.NotifyNumber)))
                {
                    var related = events.Where(a => RelatedEvents.documentExeptions.Contains(a.NotifyNumber) || RelatedEvents.constructionExeptions.Contains(a.NotifyNumber)).ToList();
                    events.AddRange(HandleRelatedEvents(related));
                }
                var reminders = (model.Reminders != null) ? model.Reminders.SelectMany(a => a.UserNotifies).ToList() : new List<UserNotifyDto>();
                
                var emailNotifyNumbers = emails.Select(a => a.NotifyNumber).ToList();
                var eventNotifyNumbers = events.Select(a => a.NotifyNumber).ToList();
                var reminderNotifyNumbers = reminders.Select(a => a.NotifyNumber).ToList();
               
                var emailNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Email && emailNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                var eventNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Event && eventNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
                var reminderNotifies = await _userNotifyRepository.Where(a => a.UserId == authenticate.UserId && a.TeamWork.ContractCode == authenticate.ContractCode && a.NotifyType == NotifyManagementType.Task && reminderNotifyNumbers.Contains(a.NotifyNumber)).ToListAsync();
            

                foreach (var item in emailNotifies)
                {

                    item.IsActive = emails.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                }
                foreach (var item in eventNotifies)
                {

                    item.IsActive = events.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                   
                }
                foreach (var item in reminderNotifies)
                {
                    item.IsActive = reminders.Where(a => a.NotifyNumber == item.NotifyNumber && a.NotifyType == item.NotifyType).First().IsActive;
                }
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
        private List<UserNotifyResultListDto> PrepareUserNotify(List<UserNotifyWithSubDto> notifies, string lang,bool isEvent)
        {
            List<UserNotifyResultListDto> result = new List<UserNotifyResultListDto>();
            List<UserNotifyDto> documentNotifies = new List<UserNotifyDto>();
            List<UserNotifyDto> procurementNotifies = new List<UserNotifyDto>();
            List<UserNotifyDto> constructionNotifies = new List<UserNotifyDto>();
            
            foreach (var item in notifies.Where(a => SCMRoleSubModule.Document.Contains($",{a.SubModuleName},")))
            {
                if (isEvent&&RelatedEvents.documentExeptions.Contains(item.NotifyNumber))
                {
                    if ((item.NotifyNumber == (int)NotifEvent.AddComTeamComment && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddComTeamCommentReply))
                    || (item.NotifyNumber == (int)NotifEvent.AddComTeamCommentReply && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddComTeamComment)))
                    {
                        documentNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                    }
                    else if ((item.NotifyNumber == (int)NotifEvent.AddTQTeamComment && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddTQTeamCommentReply))
                        || (item.NotifyNumber == (int)NotifEvent.AddTQTeamCommentReply && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddTQTeamComment)))
                    {
                        documentNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                    }
                    else if ((item.NotifyNumber == (int)NotifEvent.AddNCRTeamComment && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddNCRTeamCommentReply))
                        || (item.NotifyNumber == (int)NotifEvent.AddNCRTeamCommentReply && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddNCRTeamComment)))
                    {
                        documentNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                    }
                    else if ((item.NotifyNumber == (int)NotifEvent.AddRevisionComment && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.ReplayRevisionComment))
                        || item.NotifyNumber == (int)NotifEvent.ReplayRevisionComment && !documentNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddRevisionComment))
                    {
                        documentNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                    }
                }
                else
                {
                    if (!documentNotifies.Any(a => a.NotifyNumber == item.NotifyNumber))
                        documentNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                }

                
            }
            foreach (var item in notifies.Where(a => SCMRoleSubModule.Procurement.Contains($",{a.SubModuleName},")))
            {

                if (!procurementNotifies.Any(a => a.NotifyNumber == item.NotifyNumber))
                    procurementNotifies.Add(new UserNotifyDto
                    {
                        Id = item.Id,
                        IsActive = item.IsActive,
                        NotifyNumber = item.NotifyNumber,
                        NotifyType = item.NotifyType,
                        Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                    });

            }
            foreach (var item in notifies.Where(a => SCMRoleSubModule.Construction.Contains($",{a.SubModuleName},")))
            {
                if(isEvent&& RelatedEvents.constructionExeptions.Contains(item.NotifyNumber))
                {
                    if ((item.NotifyNumber == (int)NotifEvent.AddCommentInOperation && !constructionNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.CommentReplyInOperation))
                                       || (item.NotifyNumber == (int)NotifEvent.CommentReplyInOperation && !constructionNotifies.Any(a => a.NotifyNumber == (int)NotifEvent.AddCommentInOperation)))
                    {
                        constructionNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                    }
                }
                else
                {
                    if (!constructionNotifies.Any(a => a.NotifyNumber == item.NotifyNumber))
                        constructionNotifies.Add(new UserNotifyDto
                        {
                            Id = item.Id,
                            IsActive = item.IsActive,
                            NotifyNumber = item.NotifyNumber,
                            NotifyType = item.NotifyType,
                            Description = HandleDescription(item.NotifyType, item.NotifyNumber, lang)
                        });
                }
                
                

            }
            if (documentNotifies != null && documentNotifies.Any())
                result.Add(new UserNotifyResultListDto { Module = (lang == "en") ? ModuleType.Documents.GetDisplayName(): ModuleType.Documents.GetEnumDescription(), UserNotifies = documentNotifies });
            if (procurementNotifies != null && procurementNotifies.Any())
                result.Add(new UserNotifyResultListDto { Module = (lang == "en") ? ModuleType.Procurement.GetDisplayName() : ModuleType.Procurement.GetEnumDescription(), UserNotifies = procurementNotifies });
            if (constructionNotifies != null && constructionNotifies.Any())
                result.Add(new UserNotifyResultListDto { Module = (lang == "en") ? ModuleType.Construction.GetDisplayName() : ModuleType.Construction.GetEnumDescription(), UserNotifies = constructionNotifies });


            return result;
        }

        private string HandleDescription(NotifyManagementType type, int number, string lang)
        {
            if (lang == "en")
            {
                return (type == NotifyManagementType.Email) ? ((EmailNotify)number).GetDisplayName() : (type == NotifyManagementType.Task) ? ((TaskNotify)number).GetDisplayName() : ((NotifEvent)number).GetDisplayName();
            }
            else
            {
                return (type == NotifyManagementType.Email) ? ((EmailNotify)number).GetEnumDescription() : (type == NotifyManagementType.Task) ? ((TaskNotify)number).GetEnumDescription() : ((NotifEvent)number).GetEnumDescription();
            }
        }
        private List<UserNotifyDto> HandleRelatedEvents(List<UserNotifyDto> relateEvents)
        {
            List<UserNotifyDto> result = new List<UserNotifyDto>();
            foreach (var item in relateEvents)
            {
                if(item.NotifyNumber== (int)NotifEvent.AddComTeamComment)
                        result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddComTeamCommentReply });
                else if (item.NotifyNumber == (int)NotifEvent.AddComTeamCommentReply)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddComTeamComment });
                else if (item.NotifyNumber == (int)NotifEvent.AddTQTeamComment)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddTQTeamCommentReply });
                else if (item.NotifyNumber == (int)NotifEvent.AddTQTeamCommentReply)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddTQTeamComment });
                else if (item.NotifyNumber == (int)NotifEvent.AddNCRTeamComment)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddNCRTeamCommentReply });
                else if (item.NotifyNumber == (int)NotifEvent.AddNCRTeamCommentReply)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddNCRTeamComment });
                else if (item.NotifyNumber == (int)NotifEvent.AddRevisionComment)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.ReplayRevisionComment });
                else if (item.NotifyNumber == (int)NotifEvent.ReplayRevisionComment)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddRevisionComment });
                else if (item.NotifyNumber == (int)NotifEvent.AddCommentInOperation)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.CommentReplyInOperation });
                else if (item.NotifyNumber == (int)NotifEvent.CommentReplyInOperation)
                    result.Add(new UserNotifyDto { IsActive = item.IsActive, NotifyType = item.NotifyType, NotifyNumber = (int)NotifEvent.AddCommentInOperation });
            }

            return result;



        }
    }
}
