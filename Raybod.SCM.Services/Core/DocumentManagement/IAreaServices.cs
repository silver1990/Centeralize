using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Services.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Raybod.SCM.DataTransferObject;

namespace Raybod.SCM.Services.Core.DocumentManagement
{
    public interface IAreaServices
    {
        Task<ServiceResult<List<AreaReadDTO>>> GetAreaListAsync(AuthenticateDto authenticate);

        Task<ServiceResult<AreaReadDTO>> AddAreaAsync(AuthenticateDto authenticate, AreaAddDTO model);

        Task<ServiceResult<bool>> EditAreaByAreaIdAsync(AuthenticateDto authenticate, int areaId, AreaAddDTO model);

        Task<ServiceResult<bool>> DeleteAreaAsync(AuthenticateDto authenticate, int areaId);
    }
}
