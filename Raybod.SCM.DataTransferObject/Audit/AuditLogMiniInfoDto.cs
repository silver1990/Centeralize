using Raybod.SCM.Domain.Enum;
using System;

namespace Raybod.SCM.DataTransferObject.Audit
{
    public class AuditLogMiniInfoDto
    {
        public Guid Id { get; set; }
        public int? PerformerUserId { get; set; }

        public string PerformerUserName { get; set; }

        public string PerformerUserImage { get; set; }

        public long DateCreate { get; set; }

        public bool IsSeen { get; set; }

        public string FormCode { get; set; }

        public string Description { get; set; }

        public string Quantity { get; set; }

        public string RootKeyValue { get; set; }

        public string RootKeyValue2 { get; set; }

        public string Temp { get; set; }

        public NotifEvent NotifEvent { get; set; }

        public string Message { get; set; }

        public string KeyValue { get; set; }

        public string BaseContractCode { get; set; }

        public SCMEntityEnum SCMEntityEnum { get; set; }
        public string EventNumber { get; set; }
        public bool  IsPin { get; set; }

    }
}
