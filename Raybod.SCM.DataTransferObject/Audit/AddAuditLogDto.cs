using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Audit
{
    public class AddAuditLogDto : IAuditLogObject
    {

        public NotifEvent NotifEvent { get; set; }

        public string ContractCode { get; set; }

        public string PerformerUserFullName { get; set; }

        public string FormCode { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

        public string Quantity { get; set; }

        [MaxLength(44)]
        public string KeyValue { get; set; }

        [MaxLength(44)]
        public string RootKeyValue { get; set; }

        public string RootKeyValue2 { get; set; }

        public string Temp { get; set; }

        public List<int> ReceiverLogUserIds { get; set; }

        public int? ProductGroupId { get; set; }

        public int? DocumentGroupId { get; set; }
        public int? OperationGroupId { get; set; }
        public int PerformerUserId { get; set; }
        public string EventNumber { get; set; }

        public AddAuditLogDto()
        {
            ReceiverLogUserIds = new List<int>();
        }

    }
}
