using System;
using System.Threading.Tasks;
using NETCore.MailKit;
using MimeKit;
using System.IO;
using NETCore.MailKit.Core;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.DataTransferObject;
using System.Collections.Generic;
using System.Linq;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Utility.Helpers;
using Raybod.SCM.Utility.FileHelper;
using Exon.TheWeb.Service.Core;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.DataAccess.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using Raybod.SCM.DataTransferObject.Email;
using Raybod.SCM.Services.Core;
using Raybod.SCM.DataTransferObject._PanelDocument.Communication.Comment;
using Microsoft.Extensions.Options;
using Raybod.SCM.Utility.Extention;
using Newtonsoft.Json;

namespace Raybod.SCM.Services.Utilitys.MailService
{
    public class AppEmailService : EmailService, IAppEmailService
    {


        private readonly IMailKitProvider _mailKitProvider;
        private readonly IFileService _fileService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IEmailErrorLogService _emailErrorLogService;
        private readonly CompanyAppSettingsDto _appSettings;
        private readonly Utilitys.FileHelper _fileHelper;

        public AppEmailService(IMailKitProvider provider,
            IFileService fileService, IOptions<CompanyAppSettingsDto> appSettings,
             IWebHostEnvironment hostingEnvironmentRoot, IConfiguration configuration, IViewRenderService viewRenderService, IEmailErrorLogService emailErrorLogService) : base(provider)
        {
            _fileService = fileService;
            _mailKitProvider = provider;
            _fileHelper = new Utilitys.FileHelper(hostingEnvironmentRoot);
            _viewRenderService = viewRenderService;
            _appSettings = appSettings.Value;
            _emailErrorLogService = emailErrorLogService;
        }

        public async Task<bool> SendAsync(MimeMessage message)
        {
            try
            {
                
                
                message.From.Add(new MailboxAddress(_mailKitProvider.Options.SenderName, _mailKitProvider.Options.SenderEmail));
                using (var emailClient = _mailKitProvider.SmtpClient)
                {
                    if (!emailClient.IsConnected)
                    {
                        await emailClient.AuthenticateAsync(_mailKitProvider.Options.Account,
                        _mailKitProvider.Options.Password);
                        await emailClient.ConnectAsync(_mailKitProvider.Options.Server,
                        _mailKitProvider.Options.Port, MailKit.Security.SecureSocketOptions.None);
                    }
                    await emailClient.SendAsync(message);
                    await emailClient.DisconnectAsync(true);
                }
                

                return true;
            }
            catch (Exception ex)
            {
                EmailErrorLog error = new EmailErrorLog();
                var type = ex.GetType();
                if(type.Name== "SmtpCommandException")
                {
                    var exception = ex as MailKit.Net.Smtp.SmtpCommandException;
                    error.CreatedDate = DateTime.Now;
                    error.Message = exception.Message;
                    error.StackTrace = exception.StackTrace;
                    

                    
                    if (ex.Message== "Message size exceeds maximum permitted"&&exception.Mailbox.Address== _mailKitProvider.Options.SenderEmail)
                    {
                        await CreateExceptionSendEmailWithInvalidAttachmentObject(message.Subject.ToString());
                        
                    }
                   

                    else if (message.To.ToString() == exception.Mailbox.Address)
                    {
                        await CreateExceptionSendEmailObject(exception.Mailbox.Address);
                    }
                    else
                    {
                        await _emailErrorLogService.InsertError(error);
                        if(!message.Cc.Mailboxes.Any(a => a.Address == exception.Mailbox.Address))
                        {
                            await CreateExceptionSendEmailObject(exception.Mailbox.Address);
                            return false;
                        }
                            
                        var newCcs = message.Cc.Mailboxes.Where(a => a.Address != exception.Mailbox.Address).ToList();
                        if(newCcs.Count==0)
                        {
                            await CreateExceptionSendEmailObject(exception.Mailbox.Address);
                            MimeMessage mimeMessage = new MimeMessage();
                            mimeMessage.To.Add(new MailboxAddress(message.To.Mailboxes.First().Address));
                            mimeMessage.From.Add(new MailboxAddress(_mailKitProvider.Options.SenderName, _mailKitProvider.Options.SenderEmail));
                            mimeMessage.Body = message.Body;
                            mimeMessage.Subject = message.Subject;
                            return await SendAsync(mimeMessage);
                        }
                        else
                        {
                            await CreateExceptionSendEmailObject(exception.Mailbox.Address);
                            MimeMessage mimeMessage = new MimeMessage();
                            mimeMessage.To.Add(new MailboxAddress(message.To.Mailboxes.First().Address));
                            mimeMessage.Body = message.Body;
                            mimeMessage.Subject = message.Subject;
                            mimeMessage.From.Add(new MailboxAddress(_mailKitProvider.Options.SenderName, _mailKitProvider.Options.SenderEmail));

                            foreach (var item in newCcs)
                            {
                                mimeMessage.Cc.Add(new MailboxAddress(item.Address));
                            }
                          return await  SendAsync(mimeMessage);
                        }
                        

                    }
                }
                error.ErrorCode = "";
                error.CreatedDate = DateTime.Now;
                error.Message = ex.Message;
                error.StackTrace = ex.StackTrace;
                await _emailErrorLogService.InsertError(error);

                return false;
            }
        }
        public async Task CreateExceptionSendEmailObject(string emailAddress)
        {
            try
            {
                var CCs = _appSettings.ReportExceptionEmailCC;
                string message = $"ارسال ایمیل به آدرس {emailAddress} با خطا مواجه شد";
                CommentMentionNotif emaiBody = new CommentMentionNotif(message, null, new List<CommentNotifViaEmailDTO>(), _appSettings.CompanyName);
                var emailRequest = new SendEmailDto
                {
                    To = _appSettings.ReportExceptionEmailTo,
                    Body = await _viewRenderService.RenderToStringAsync("_TransmittlaNotifEmailFree", emaiBody),
                    Subject = "Email Exception",
                    CCs=CCs
                };
                Hangfire.BackgroundJob.Enqueue(()=>SendAsync(emailRequest,null,false,""));
            }
           catch(Exception ex)
            {

            }
        }
        public async Task CreateExceptionSendEmailWithInvalidAttachmentObject(string emailsubbjec)
        {
            try
            {
                var CCs = _appSettings.ReportExceptionEmailCC;
                string message = $"ارسال ایمیل {emailsubbjec} به دلیل حجم غیر مجاز با خطا مواجه شد";
                CommentMentionNotif emaiBody = new CommentMentionNotif(message, null, new List<CommentNotifViaEmailDTO>(), _appSettings.CompanyName);
                var emailRequest = new SendEmailDto
                {
                    To = _appSettings.ReportExceptionEmailTo,
                    Body = await _viewRenderService.RenderToStringAsync("_TransmittlaNotifEmailFree", emaiBody),
                    Subject = "Email Exception",
                    CCs = CCs
                };
                Hangfire.BackgroundJob.Enqueue(() => SendAsync(emailRequest, null, false, ""));
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<ServiceResult<bool>> SendAsync(EmailRequest emailRequest)
        {
            try
            {
                //emailRequest = new EmailRequest
                //{
                //    Attachment = null,
                //    Body = "hello new World",
                //    Subject = "new config mail",
                //    ToAddress = "mehdi.rahimi@raybodravesh.com"
                //};
                MimeMessage mimeMessage = new MimeMessage();

                if (emailRequest.To.Any())
                    mimeMessage.To.AddRange(emailRequest.To.Select(email => new MailboxAddress(email)).ToList());

                if (emailRequest.Bcc.Any())
                    mimeMessage.To.AddRange(emailRequest.Bcc.Select(email => new MailboxAddress(email)).ToList());

                mimeMessage.Subject = emailRequest.Subject;
                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                if (emailRequest.Attachment != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await emailRequest.Attachment.CopyToAsync(memoryStream);
                        builder.Attachments.Add(emailRequest.Attachment.FileName, memoryStream.ToArray());
                    }
                }

                mimeMessage.Body = builder.ToMessageBody();
                var result = await SendAsync(mimeMessage);
                return result
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }

        }

        public async Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest, List<InMemoryFileDto> attachments = null, bool isSendAttachArchiveZip = false,string attachmentName= "attach.zip")
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                if(!string.IsNullOrEmpty(emailRequest.To))
                mimeMessage.To.Add(new MailboxAddress(emailRequest.To));
                
                foreach (var item in emailRequest.Tos)
                {
                    mimeMessage.To.Add(new MailboxAddress(item));
                }

                mimeMessage.Subject = emailRequest.Subject;

                if (!string.IsNullOrEmpty(emailRequest.CC))
                {
                    mimeMessage.Cc.Add(new MailboxAddress(emailRequest.CC));
                }

                foreach (var item in emailRequest.CCs)
                {
                    mimeMessage.Cc.Add(new MailboxAddress(item));
                }

                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                if (attachments != null && attachments.Count() > 0)
                {
                    if (isSendAttachArchiveZip)
                    {
                        var res = await AddAttachmentToBodyBuilderArchiveZip(builder, attachments,attachmentName);
                        if (res == null)
                            return ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
                        builder = res;

                    }
                    else
                        builder = AddAttachmentToBodyBuilder(builder, attachments);

                }
                mimeMessage.Body = builder.ToMessageBody();
                var result = await SendAsync(mimeMessage);
                return result
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest,MemoryStream attachment,string attachmentName )
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                if (!string.IsNullOrEmpty(emailRequest.To))
                    mimeMessage.To.Add(new MailboxAddress(emailRequest.To));

                foreach (var item in emailRequest.Tos)
                {
                    mimeMessage.To.Add(new MailboxAddress(item));
                }

                mimeMessage.Subject = emailRequest.Subject;

                if (!string.IsNullOrEmpty(emailRequest.CC))
                {
                    mimeMessage.Cc.Add(new MailboxAddress(emailRequest.CC));
                }

                foreach (var item in emailRequest.CCs)
                {
                    mimeMessage.Cc.Add(new MailboxAddress(item));
                }

                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                if (attachment != null && attachment.Length > 0)
                {
                    builder.Attachments.Add(attachmentName,attachment);

                }
                mimeMessage.Body = builder.ToMessageBody();
                var result = await SendAsync(mimeMessage);
                return result
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendTransmittalEmailAsync(SendEmailDto emailRequest, TransmittalFilesDto attachments = null,  string attachmentName = "attach.zip")
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                if (!string.IsNullOrEmpty(emailRequest.To))
                    mimeMessage.To.Add(new MailboxAddress(emailRequest.To));

                foreach (var item in emailRequest.Tos)
                {
                    mimeMessage.To.Add(new MailboxAddress(item));
                }

                mimeMessage.Subject = emailRequest.Subject;

                if (!string.IsNullOrEmpty(emailRequest.CC))
                {
                    mimeMessage.Cc.Add(new MailboxAddress(emailRequest.CC));
                }

                foreach (var item in emailRequest.CCs)
                {
                    mimeMessage.Cc.Add(new MailboxAddress(item));
                }

                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                if (attachments != null && ((attachments.RevisionFiles!=null&&attachments.RevisionFiles.Count() > 0)||attachments.TransmitallFile!=null))
                {
                    
                        var res = await AddAttachmentToBodyBuilderArchiveZipForTransmittal(builder, attachments, attachmentName);
                        if (res == null)
                            return ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
                        builder = res;

  

                }
                mimeMessage.Body = builder.ToMessageBody();
                var result = await SendAsync(mimeMessage);
                return result
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }
        public async Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest, string attachments)
        {
            try
            {
                MimeMessage mimeMessage = new MimeMessage();

                if(!string.IsNullOrEmpty(emailRequest.To))
                mimeMessage.To.Add(new MailboxAddress(emailRequest.To));
                
                foreach (var item in emailRequest.Tos)
                {
                    mimeMessage.To.Add(new MailboxAddress(item));
                }

                mimeMessage.Subject = emailRequest.Subject;

                if (!string.IsNullOrEmpty(emailRequest.CC))
                {
                    mimeMessage.Cc.Add(new MailboxAddress(emailRequest.CC));
                }

                foreach (var item in emailRequest.CCs)
                {
                    mimeMessage.Cc.Add(new MailboxAddress(item));
                }

                var builder = new BodyBuilder { HtmlBody = emailRequest.Body };
                if (!String.IsNullOrEmpty(attachments)&&File.Exists(attachments))
                {
                    builder.Attachments.Add(attachments);
                   
                }
                else
                {
                    return ServiceResultFactory.CreateError(false, MessageId.FileNotFound);
                }
                mimeMessage.Body = builder.ToMessageBody();
                var result = await SendAsync(mimeMessage);
                return result
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SendMailFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        private BodyBuilder AddAttachmentToBodyBuilder(BodyBuilder builder, List<InMemoryFileDto> sendAttachmentUrl)
        {
            if (sendAttachmentUrl != null && sendAttachmentUrl.Count() > 0)
            {
                foreach (var item in sendAttachmentUrl)
                {
                    builder.Attachments.Add(_fileHelper.FileReadSrc(item.FileName, item.FileUrl));
                }
            }
            return builder;
        }

        private async Task<BodyBuilder> AddAttachmentToBodyBuilderArchiveZip(BodyBuilder builder, List<InMemoryFileDto> attachments,string attachmentName)
        {
            var res = await _fileHelper.ToMemoryStreamZipFileAsync(attachments);
            if (res == null)
                return null;
            
            builder.Attachments.Add(attachmentName, res.Stream.ToArray());
            return builder;
        }
        private async Task<BodyBuilder> AddAttachmentToBodyBuilderArchiveZipForTransmittal(BodyBuilder builder, TransmittalFilesDto attachments, string attachmentName)
        {
            var res = await _fileHelper.ToMemoryStreamZipFileForTransmittalAsync(attachments);
            if (res == null)
                return null;

            builder.Attachments.Add(attachmentName, res.Stream.ToArray());
            return builder;
        }
        public ServiceResult<bool> IsValidEmailRequest(SendEmailDto emailRequest, List<string> cc = null)
        {

            if (!RegexHelpers.IsValidEmail(emailRequest.To))
                return ServiceResultFactory.CreateError(false, MessageId.InvalidEmailTo);

            if (cc != null && cc.Count() > 0)
            {
                foreach (var email in cc)
                {
                    if (!RegexHelpers.IsValidEmail(email))
                        return ServiceResultFactory.CreateError(false, MessageId.InvalidEmailCC);
                }
            }

            if (string.IsNullOrEmpty(emailRequest.Subject))
                return ServiceResultFactory.CreateError(false, MessageId.EmailSubjectIsRequeird);


            if (string.IsNullOrEmpty(emailRequest.Body))
                return ServiceResultFactory.CreateError(false, MessageId.EmailBodyIsRequeird);

            return ServiceResultFactory.CreateSuccess(true);
        }
    }

}
