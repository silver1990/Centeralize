using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Audit
{
    public interface IAuditLogObject
    {
        NotifEvent NotifEvent { get; set; }

        string PerformerUserFullName { get; set; }

        int PerformerUserId { get; set; }

        string FormCode { get; set; }

        string ContractCode { get; set; }

        string Description { get; set; }

        string KeyValue { get; set; }

        string RootKeyValue { get; set; }

        string Quantity { get; set; }

    }
}
