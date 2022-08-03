using System;
using System.Collections.Generic;
using System.Text;

namespace Raybod.SCM.DataTransferObject.PendingForPayment
{
    public class AddPendingForPaymentResultDto
    {
        public PendingForPaymentInfoDto PendingForPaymentInfo { get; set; }
        public List<RequestedAmountOFPOPaymentStepDto> RequestPaymentStepsInfo { get; set; }
        public AddPendingForPaymentResultDto()
        {
            RequestPaymentStepsInfo = new List<RequestedAmountOFPOPaymentStepDto>();
        }

    }
}
