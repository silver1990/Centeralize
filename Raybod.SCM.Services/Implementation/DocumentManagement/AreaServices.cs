using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject._PanelDocument.Area;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Services.Core.Common;
using Raybod.SCM.Services.Core.Common.Message;
using Raybod.SCM.Services.Core.DocumentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Implementation.DocumentManagement
{
    public class AreaServices : IAreaServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DbSet<Area> _areaRepository;


        public AreaServices(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _areaRepository = _unitOfWork.Set<Area>();
        }

        public async Task<ServiceResult<AreaReadDTO>> AddAreaAsync(AuthenticateDto authenticate, AreaAddDTO model)
        {
            var messages = new List<ServiceMessage>();
            try
            {


                if(await _areaRepository.AnyAsync(a => a.ContractCode == authenticate.ContractCode && a.AreaTitle == model.AreaTitle.Trim() && !a.IsDeleted))
                {
                    return ServiceResultFactory.CreateError<AreaReadDTO>(null, MessageId.AreaExistAlready);
                }
                if(model.AreaTitle.Length>20)
                {
                    return ServiceResultFactory.CreateError<AreaReadDTO>(null, MessageId.AreaTitleLegnthOverLimited);
                }
                var areaModel = new Area();
                areaModel.AreaTitle = model.AreaTitle.Trim();
                areaModel.ContractCode = authenticate.ContractCode;
                areaModel.IsDeleted = false;
                areaModel.AdderUserId = authenticate.UserId;
                areaModel.ModifierUserId = authenticate.UserId;
                areaModel.CreatedDate = DateTime.Now;
                areaModel.UpdateDate = DateTime.Now;


                _areaRepository.Add(areaModel);

                if (await _unitOfWork.SaveChangesAsync() > 0)
                {
                    var result = new AreaReadDTO { AreaId=areaModel.AreaId,AreaTitle=areaModel.AreaTitle};
                    return ServiceResultFactory.CreateSuccess(result);
                }

                return ServiceResultFactory.CreateError(new AreaReadDTO(), MessageId.SaveFailed);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException<AreaReadDTO>(null, exception);
            }
        }

        public async Task<ServiceResult<bool>> DeleteAreaAsync(AuthenticateDto authenticate, int areaId)
        {
            try
            {
                var selectedArea = await _areaRepository.FindAsync(areaId);
                if (selectedArea == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);
                if (selectedArea.Documents != null && selectedArea.Documents.Any()||selectedArea.BomProducts!=null&&selectedArea.BomProducts.Any())
                {
                    return ServiceResultFactory.CreateError(false, MessageId.DeleteEntityFailed);
                }

                selectedArea.IsDeleted = true;
                selectedArea.ModifierUserId = authenticate.UserId;
                selectedArea.UpdateDate = DateTime.Now;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<bool>> EditAreaByAreaIdAsync(AuthenticateDto authenticate, int areaId, AreaAddDTO model)
        {
            try
            {
                var selectedArea = await _areaRepository.FindAsync(areaId);
                if (selectedArea == null)
                    return ServiceResultFactory.CreateError(false, MessageId.EntityDoesNotExist);

                selectedArea.AreaTitle = model.AreaTitle;
                selectedArea.ModifierUserId = authenticate.UserId;
                selectedArea.UpdateDate = DateTime.Now;
                return await _unitOfWork.SaveChangesAsync() > 0
                    ? ServiceResultFactory.CreateSuccess(true)
                    : ServiceResultFactory.CreateError(false, MessageId.SaveFailed);
            }
            catch (System.Exception exception)
            {
                return ServiceResultFactory.CreateException(false, exception);
            }
        }

        public async Task<ServiceResult<List<AreaReadDTO>>> GetAreaListAsync(AuthenticateDto authenticate)
        {
            try
            {
                var dbQuery = _areaRepository.Where(a => !a.IsDeleted && a.ContractCode == authenticate.ContractCode);




                var areas = await dbQuery.ToListAsync();

                var mapperConfiguration = new MapperConfiguration(configuration =>
                {
                    configuration.CreateMap<Area, AreaReadDTO>();

                });

                var mapper = mapperConfiguration.CreateMapper();
                var areaModel = mapper.Map<List<AreaReadDTO>>(areas);
                return ServiceResultFactory.CreateSuccess(areaModel);
            }
            catch (Exception exception)
            {
                return ServiceResultFactory.CreateException(new List<AreaReadDTO>(), exception);
            }
        }
    }
}
