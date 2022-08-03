using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class PendingForPaymentDto
    {
        public List<PendingForPaymentInfoDto> PendingForPaymentInfo { get; set; }
        public List<RequestedAmountOFPOPaymentStepDto> RequestPaymentStepsInfo { get; set; }
        public PendingForPaymentDto()
        {
            RequestPaymentStepsInfo = new List<RequestedAmountOFPOPaymentStepDto>();
            PendingForPaymentInfo = new List<PendingForPaymentInfoDto>();
        }
    }
}
