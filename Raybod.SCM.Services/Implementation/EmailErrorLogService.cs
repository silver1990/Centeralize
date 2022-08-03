using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation
{
    public class EmailErrorLogService : IEmailErrorLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<EmailErrorLog> _emailErrorLogRepository;
        public EmailErrorLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _emailErrorLogRepository = _unitOfWork.Set<EmailErrorLog>();
        }

        public async Task InsertError(EmailErrorLog model)
        {
            try
            {
                await _emailErrorLogRepository.AddAsync(model);
                await _unitOfWork.SaveChangesAsync();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
