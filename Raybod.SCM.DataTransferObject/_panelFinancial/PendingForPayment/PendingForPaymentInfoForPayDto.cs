using Raybod.SCM.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class PendingForPaymentInfoForPayDto
    {
        public long PRContractId { get; set; }
        public string ContractCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierLogo { get; set; }
        public int SupplierId { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public List<PendingForPaymentForPayedDto> PendingForPayments { get; set; }
        public PendingForPaymentInfoForPayDto()
        {
            PendingForPayments = new List<PendingForPaymentForPayedDto>();
        }
    }
}
