using Raybod.SCM.DataTransferObject.PendingForPayment;
using Raybod.SCM.DataTransferObject.Supplier;
using System.Collections.Generic;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class PendingForPaymentListForAddNewPaymentDto : BasePaymentDto
    {
        public List<ListPendingForPaymentDto> PendingForPayments { get; set; }

        public SupplierInformationDto SupplierInfo { get; set; }

    }
}
