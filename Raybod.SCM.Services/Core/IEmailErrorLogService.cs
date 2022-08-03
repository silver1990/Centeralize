using Raybod.SCM.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Core
{
    public interface IEmailErrorLogService
    {
        Task InsertError(EmailErrorLog model);
    }
}
