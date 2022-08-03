using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.FileDriveShare
{
    public class FileDriveShareCreateDto
    {
        public Guid EntityId { get; set; }
        public EntityType EntityType { get; set; }
        public List<FileDriveShareUserAccessablityDto> Users { get; set; }
        public List<FileDriveShareOwnerUserAccessablityDto> Owners { get; set; }
        public FileDriveShareCreateDto()
        {
            Users = new List<FileDriveShareUserAccessablityDto>();
            Owners = new List<FileDriveShareOwnerUserAccessablityDto>();
        }
    }
}
