using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MimeKit;
using NETCore.MailKit.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Utilitys.MailService.Model;
using Raybod.SCM.Utility.FileHelper;

namespace Raybod.SCM.Services.Utilitys.MailService
{
    public interface IAppEmailService : IEmailService
    {
        Task<bool> SendAsync(MimeMessage message);

        Task<ServiceResult<bool>> SendAsync(EmailRequest emailRequest);

        Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest, List<InMemoryFileDto> attachments = null, bool isSendAttachArchiveZip = false,string attachmentName="attach.zip");
        Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest, string attachments );
        Task<ServiceResult<bool>> SendAsync(SendEmailDto emailRequest, MemoryStream attachment, string attachmentName);
        ServiceResult<bool> IsValidEmailRequest(SendEmailDto emailRequest, List<string> cc = null);
        Task<ServiceResult<bool>> SendTransmittalEmailAsync(SendEmailDto emailRequest, TransmittalFilesDto attachments = null, string attachmentName = "attach.zip");
    }
}
