using Raybod.SCM.Domain.Enum;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Payment
{
    public class AddPaymentDto
    {

        public AddPaymentConfirmationDto WorkFlow { get; set; }

        public List<PaymentSubjectDto> PendingForPayments { get; set; }
    }
}
